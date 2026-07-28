-- ============================================================================
-- BloodCenterOS — Patch 20260724-009: Financials — Invoicing, Dues Register, MBB Billing
-- Description: Invoice detail view, dues register for credit collection,
--   credit notes, and Mother Blood Bank billing (payables).
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_009_financials.sql
-- ============================================================================

-- 1. MBB Billing Tables ----------------------------------------------------

CREATE TABLE IF NOT EXISTS MbbBill (
    MbbBillId    BIGSERIAL PRIMARY KEY,
    CenterId     BIGINT NOT NULL DEFAULT 0,
    BillNumber   VARCHAR(100) NOT NULL,
    BillDate     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    SupplierName VARCHAR(300),
    TotalAmount  NUMERIC(18,2) DEFAULT 0,
    PaymentMode  VARCHAR(50),              -- Cash, Credit, Cheque
    PaymentStatus VARCHAR(50) DEFAULT 'Pending',
    ChequeNo     VARCHAR(100),
    ChequeDate   DATE,
    Notes        VARCHAR(2000),
    CreatedBy    BIGINT,
    CreatedAt    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS MbbBillDetail (
    MbbBillDetailId BIGSERIAL PRIMARY KEY,
    MbbBillId        BIGINT NOT NULL REFERENCES MbbBill(MbbBillId) ON DELETE CASCADE,
    ComponentType    VARCHAR(100) NOT NULL,
    BloodGroup       VARCHAR(20),
    Quantity         INT NOT NULL DEFAULT 1,
    UnitPrice        NUMERIC(18,2) DEFAULT 0,
    LineTotal        NUMERIC(18,2) GENERATED ALWAYS AS (Quantity * UnitPrice) STORED,
    BagNumbers       VARCHAR(1000)         -- Comma-separated or JSON array of bag IDs
);

-- 2. Billing Enhancement SPs ------------------------------------------------

CREATE OR REPLACE FUNCTION fn_billing_get_by_id(p_billing_id BIGINT)
RETURNS TABLE(
    BillingTransactionId BIGINT, CenterId BIGINT, InvoiceNumber VARCHAR,
    PatientId BIGINT, PatientName VARCHAR, TotalAmount NUMERIC, TaxAmount NUMERIC,
    Discount NUMERIC, PaymentStatus VARCHAR, PaymentMode VARCHAR,
    InvoiceDate TIMESTAMPTZ, CreatedAt TIMESTAMPTZ, CreatedBy BIGINT
) AS $$
BEGIN
    RETURN QUERY SELECT b.BillingTransactionId, b.CenterId, b.InvoiceNumber,
        b.PatientId, NULL::VARCHAR, b.TotalAmount, b.TaxAmount, b.Discount,
        b.PaymentStatus, b.PaymentMode, b.InvoiceDate, b.CreatedAt, b.CreatedBy
    FROM BillingTransaction b
    WHERE b.BillingTransactionId = p_billing_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_billing_get_detail(p_billing_id BIGINT)
RETURNS TABLE(
    InvoiceDetailId BIGINT, BillingTransactionId BIGINT, ComponentId BIGINT,
    ServiceName VARCHAR, Quantity INT, UnitPrice NUMERIC, LineTotal NUMERIC
) AS $$
BEGIN
    RETURN QUERY SELECT d.InvoiceDetailId, d.BillingTransactionId, d.ComponentId,
        d.ServiceName, d.Quantity, d.UnitPrice, d.LineTotal
    FROM InvoiceDetail d
    WHERE d.BillingTransactionId = p_billing_id
    ORDER BY d.InvoiceDetailId;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_billing_get_dues(
    p_center_id BIGINT,
    p_patient_name VARCHAR DEFAULT NULL
)
RETURNS TABLE(
    BillingTransactionId BIGINT, InvoiceNumber VARCHAR, PatientName VARCHAR,
    TotalAmount NUMERIC, PaidAmount NUMERIC, Balance NUMERIC,
    InvoiceDate TIMESTAMPTZ, PaymentStatus VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    WITH payment_summary AS (
        SELECT p.BillingTransactionId,
            COALESCE(SUM(p.Amount), 0) AS paid
        FROM PaymentRecord p
        GROUP BY p.BillingTransactionId
    )
    SELECT b.BillingTransactionId, b.InvoiceNumber,
        b.PatientId::VARCHAR AS PatientName,
        b.TotalAmount,
        COALESCE(ps.paid, 0) AS PaidAmount,
        b.TotalAmount - COALESCE(ps.paid, 0) AS Balance,
        b.InvoiceDate, b.PaymentStatus
    FROM BillingTransaction b
    LEFT JOIN payment_summary ps ON ps.BillingTransactionId = b.BillingTransactionId
    WHERE b.CenterId = p_center_id
        AND b.PaymentStatus IN ('Pending', 'Partial', 'Credit')
        AND (p_patient_name IS NULL OR b.InvoiceNumber ILIKE '%' || p_patient_name || '%')
    ORDER BY b.InvoiceDate;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_billing_credit_note(
    p_center_id BIGINT,
    p_original_invoice_id BIGINT,
    p_amount NUMERIC,
    p_reason VARCHAR,
    p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE
    v_new_id BIGINT;
    v_orig_inv VARCHAR;
BEGIN
    SELECT InvoiceNumber INTO v_orig_inv FROM BillingTransaction
    WHERE BillingTransactionId = p_original_invoice_id;

    INSERT INTO BillingTransaction (CenterId, InvoiceNumber, PatientId,
        TotalAmount, TaxAmount, Discount, PaymentStatus, PaymentMode,
        InvoiceDate, CreatedAt, CreatedBy)
    VALUES (p_center_id, 'CN-' || v_orig_inv || '-' || TO_CHAR(NOW(), 'YYYYMMDD'),
        NULL, -p_amount, 0, 0, 'Paid', 'Credit Note',
        NOW(), NOW(), p_created_by)
    RETURNING BillingTransactionId INTO v_new_id;

    INSERT INTO InvoiceDetail (BillingTransactionId, ComponentId, ServiceName,
        Quantity, UnitPrice, LineTotal)
    VALUES (v_new_id, NULL, 'Credit Note: ' || p_reason, 1, -p_amount, -p_amount);

    -- Reduce original invoice balance
    UPDATE BillingTransaction
    SET PaymentStatus = CASE
        WHEN (SELECT COALESCE(SUM(Amount),0) FROM PaymentRecord
            WHERE BillingTransactionId = p_original_invoice_id) - p_amount <= 0 THEN 'Pending'
        ELSE 'Partial'
    END
    WHERE BillingTransactionId = p_original_invoice_id;

    RETURN v_new_id;
END;
$$ LANGUAGE plpgsql;


-- 3. MBB Billing SPs --------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_mbb_bill_create(
    p_center_id BIGINT,
    p_bill_number VARCHAR,
    p_bill_date TIMESTAMPTZ,
    p_supplier_name VARCHAR,
    p_payment_mode VARCHAR,
    p_cheque_no VARCHAR,
    p_cheque_date DATE,
    p_notes VARCHAR,
    p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO MbbBill (CenterId, BillNumber, BillDate, SupplierName,
        PaymentMode, ChequeNo, ChequeDate, Notes, CreatedBy)
    VALUES (p_center_id, p_bill_number, p_bill_date, p_supplier_name,
        p_payment_mode, p_cheque_no, p_cheque_date, p_notes, p_created_by)
    RETURNING MbbBillId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_mbb_bill_add_detail(
    p_mbb_bill_id BIGINT,
    p_component_type VARCHAR,
    p_blood_group VARCHAR,
    p_quantity INT,
    p_unit_price NUMERIC,
    p_bag_numbers VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO MbbBillDetail (MbbBillId, ComponentType, BloodGroup,
        Quantity, UnitPrice, BagNumbers)
    VALUES (p_mbb_bill_id, p_component_type, p_blood_group,
        p_quantity, p_unit_price, p_bag_numbers)
    RETURNING MbbBillDetailId INTO v_id;

    UPDATE MbbBill SET TotalAmount = (
        SELECT COALESCE(SUM(LineTotal), 0) FROM MbbBillDetail
        WHERE MbbBillId = p_mbb_bill_id
    ) WHERE MbbBillId = p_mbb_bill_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_mbb_bill_get_by_center(p_center_id BIGINT)
RETURNS TABLE(
    MbbBillId BIGINT, BillNumber VARCHAR, BillDate TIMESTAMPTZ,
    SupplierName VARCHAR, TotalAmount NUMERIC, PaymentMode VARCHAR,
    PaymentStatus VARCHAR, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT b.MbbBillId, b.BillNumber, b.BillDate,
        b.SupplierName, b.TotalAmount, b.PaymentMode, b.PaymentStatus, b.CreatedAt
    FROM MbbBill b
    WHERE b.CenterId = p_center_id
    ORDER BY b.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_mbb_bill_get_by_id(p_bill_id BIGINT)
RETURNS TABLE(
    MbbBillId BIGINT, BillNumber VARCHAR, BillDate TIMESTAMPTZ,
    SupplierName VARCHAR, TotalAmount NUMERIC, PaymentMode VARCHAR,
    PaymentStatus VARCHAR, ChequeNo VARCHAR, ChequeDate DATE,
    Notes VARCHAR, CreatedBy BIGINT, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT b.MbbBillId, b.BillNumber, b.BillDate,
        b.SupplierName, b.TotalAmount, b.PaymentMode, b.PaymentStatus,
        b.ChequeNo, b.ChequeDate, b.Notes, b.CreatedBy, b.CreatedAt
    FROM MbbBill b WHERE b.MbbBillId = p_bill_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_mbb_bill_get_detail(p_bill_id BIGINT)
RETURNS TABLE(
    MbbBillDetailId BIGINT, ComponentType VARCHAR, BloodGroup VARCHAR,
    Quantity INT, UnitPrice NUMERIC, LineTotal NUMERIC, BagNumbers VARCHAR
) AS $$
BEGIN
    RETURN QUERY SELECT d.MbbBillDetailId, d.ComponentType, d.BloodGroup,
        d.Quantity, d.UnitPrice, d.LineTotal, d.BagNumbers
    FROM MbbBillDetail d
    WHERE d.MbbBillId = p_bill_id
    ORDER BY d.MbbBillDetailId;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_mbb_bill_make_payment(
    p_bill_id BIGINT,
    p_amount NUMERIC,
    p_payment_mode VARCHAR,
    p_created_by BIGINT
) RETURNS VOID AS $$
BEGIN
    UPDATE MbbBill
    SET PaymentStatus = CASE
        WHEN p_amount >= TotalAmount THEN 'Paid'
        ELSE 'Partial'
    END,
    PaymentMode = p_payment_mode
    WHERE MbbBillId = p_bill_id
      AND (PaymentStatus IS NULL OR PaymentStatus != 'Paid');
END;
$$ LANGUAGE plpgsql;
