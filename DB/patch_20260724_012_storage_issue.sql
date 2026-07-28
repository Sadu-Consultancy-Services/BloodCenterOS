-- ============================================================================
-- BloodCenterOS — Patch 20260724-012: Issue to Blood Storage Units
-- Description: Storage unit master CRUD + batch-issuing blood to external
--   storage units with auto-invoicing.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_012_storage_issue.sql
-- ============================================================================

-- 1. StorageMaster Table -----------------------------------------------------

CREATE TABLE IF NOT EXISTS StorageMaster (
    StorageId      BIGSERIAL PRIMARY KEY,
    CenterId       BIGINT NOT NULL DEFAULT 0,
    StorageName    VARCHAR(200) NOT NULL,
    Address        VARCHAR(500),
    PhoneNo        VARCHAR(50),
    Email          VARCHAR(200),
    ContactPerson  VARCHAR(200),
    ContactPhone   VARCHAR(50),
    ContactEmail   VARCHAR(200),
    RateWB         NUMERIC(18,2) DEFAULT 0,
    RatePCV        NUMERIC(18,2) DEFAULT 0,
    RateFFP        NUMERIC(18,2) DEFAULT 0,
    RatePltsConc   NUMERIC(18,2) DEFAULT 0,
    IsActive       BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedBy      BIGINT,
    CreatedAt      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. IssueStorage Table ------------------------------------------------------

CREATE TABLE IF NOT EXISTS IssueStorage (
    IssueStorageId BIGSERIAL PRIMARY KEY,
    CenterId       BIGINT NOT NULL DEFAULT 0,
    StorageId      BIGINT NOT NULL REFERENCES StorageMaster(StorageId),
    ComponentId    BIGINT NOT NULL REFERENCES ComponentMaster(ComponentId),
    InvoiceId      BIGINT REFERENCES BillingTransaction(BillingTransactionId),
    IssueDateTime  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    Rate           NUMERIC(18,2) DEFAULT 0,
    CreatedAt      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3. Storage SPs -------------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_storage_get_by_center(p_center_id BIGINT)
RETURNS TABLE(
    StorageId BIGINT, StorageName VARCHAR, Address VARCHAR, PhoneNo VARCHAR,
    Email VARCHAR, ContactPerson VARCHAR, ContactPhone VARCHAR, ContactEmail VARCHAR,
    RateWB NUMERIC, RatePCV NUMERIC, RateFFP NUMERIC, RatePltsConc NUMERIC,
    IsActive BOOLEAN, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT s.StorageId, s.StorageName, s.Address, s.PhoneNo,
        s.Email, s.ContactPerson, s.ContactPhone, s.ContactEmail,
        s.RateWB, s.RatePCV, s.RateFFP, s.RatePltsConc,
        s.IsActive, s.CreatedAt
    FROM StorageMaster s WHERE s.CenterId = p_center_id
    ORDER BY s.StorageName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_storage_get_by_id(p_id BIGINT)
RETURNS TABLE(
    StorageId BIGINT, CenterId BIGINT, StorageName VARCHAR, Address VARCHAR,
    PhoneNo VARCHAR, Email VARCHAR, ContactPerson VARCHAR, ContactPhone VARCHAR,
    ContactEmail VARCHAR, RateWB NUMERIC, RatePCV NUMERIC, RateFFP NUMERIC,
    RatePltsConc NUMERIC, IsActive BOOLEAN, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT s.StorageId, s.CenterId, s.StorageName, s.Address,
        s.PhoneNo, s.Email, s.ContactPerson, s.ContactPhone, s.ContactEmail,
        s.RateWB, s.RatePCV, s.RateFFP, s.RatePltsConc,
        s.IsActive, s.CreatedAt
    FROM StorageMaster s WHERE s.StorageId = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_storage_upsert(
    p_center_id BIGINT,
    p_id BIGINT DEFAULT NULL,
    p_name VARCHAR DEFAULT NULL,
    p_address VARCHAR DEFAULT NULL,
    p_phone VARCHAR DEFAULT NULL,
    p_email VARCHAR DEFAULT NULL,
    p_contact_person VARCHAR DEFAULT NULL,
    p_contact_phone VARCHAR DEFAULT NULL,
    p_contact_email VARCHAR DEFAULT NULL,
    p_rate_wb NUMERIC DEFAULT 0,
    p_rate_pcv NUMERIC DEFAULT 0,
    p_rate_ffp NUMERIC DEFAULT 0,
    p_rate_plts NUMERIC DEFAULT 0,
    p_is_active BOOLEAN DEFAULT TRUE,
    p_created_by BIGINT DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    IF p_id IS NOT NULL AND EXISTS (SELECT 1 FROM StorageMaster WHERE StorageId = p_id) THEN
        UPDATE StorageMaster SET StorageName = p_name, Address = p_address,
            PhoneNo = p_phone, Email = p_email, ContactPerson = p_contact_person,
            ContactPhone = p_contact_phone, ContactEmail = p_contact_email,
            RateWB = p_rate_wb, RatePCV = p_rate_pcv, RateFFP = p_rate_ffp,
            RatePltsConc = p_rate_plts, IsActive = p_is_active
        WHERE StorageId = p_id RETURNING StorageId INTO v_id;
        RETURN v_id;
    END IF;
    INSERT INTO StorageMaster (CenterId, StorageName, Address, PhoneNo, Email,
        ContactPerson, ContactPhone, ContactEmail, RateWB, RatePCV, RateFFP,
        RatePltsConc, IsActive, CreatedBy)
    VALUES (p_center_id, p_name, p_address, p_phone, p_email, p_contact_person,
        p_contact_phone, p_contact_email, p_rate_wb, p_rate_pcv, p_rate_ffp,
        p_rate_plts, p_is_active, p_created_by)
    RETURNING StorageId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_storage_delete(p_id BIGINT) RETURNS VOID AS $$
BEGIN
    UPDATE StorageMaster SET IsActive = FALSE WHERE StorageId = p_id;
END;
$$ LANGUAGE plpgsql;

-- 4. IssueStorage SPs --------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_issue_storage_get_available_components(p_center_id BIGINT)
RETURNS TABLE(
    ComponentId BIGINT, ComponentCode VARCHAR, ComponentType VARCHAR,
    BloodGroup VARCHAR, VolumeMl INT, ExpiryDate DATE, BagId BIGINT, BagNo VARCHAR
) AS $$
BEGIN
    RETURN QUERY SELECT c.componentid, c.componentcode, c.componenttype,
        d.bloodgroup, c.volumeml, c.expirydate::DATE, bg.bagid, bg.bagno
    FROM ComponentMaster c
    JOIN BloodBagMaster bg ON bg.bagid = c.bagid
    JOIN DonorMaster d ON d.donorid = bg.donorid
    WHERE c.centerid = p_center_id AND c.currentstatus = 'Available'
    ORDER BY c.expirydate;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_issue_storage_get_storage_rate(
    p_storage_id BIGINT, p_component_type VARCHAR
) RETURNS NUMERIC AS $$
DECLARE v_rate NUMERIC;
BEGIN
    SELECT CASE p_component_type
        WHEN 'WB' THEN RateWB WHEN 'PCV' THEN RatePCV
        WHEN 'FFP' THEN RateFFP WHEN 'PltsConc' THEN RatePltsConc
        ELSE 0 END INTO v_rate
    FROM StorageMaster WHERE StorageId = p_storage_id;
    RETURN COALESCE(v_rate, 0);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_issue_storage_create(
    p_center_id BIGINT,
    p_storage_id BIGINT,
    p_component_ids BIGINT[],
    p_issue_date TIMESTAMPTZ,
    p_payment_mode VARCHAR DEFAULT 'Credit',
    p_discount NUMERIC DEFAULT 0,
    p_discount_reason VARCHAR DEFAULT NULL,
    p_em_amt NUMERIC DEFAULT 0,
    p_notes VARCHAR DEFAULT NULL,
    p_created_by BIGINT DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE
    v_invoice_id BIGINT;
    v_inv_no VARCHAR;
    v_total NUMERIC := 0;
    v_rate NUMERIC;
    v_cid BIGINT;
    v_component_type VARCHAR;
    v_component_code VARCHAR;
    v_detail RECORD;
    v_bag_id BIGINT;
    v_patient_name VARCHAR;
BEGIN
    -- Generate invoice number
    v_inv_no := 'BSI-' || p_storage_id || '-' || TO_CHAR(p_issue_date, 'YYYYMMDD') || '-' || TO_CHAR(NOW(), 'HH24MISS');

    -- Get storage name for invoice
    SELECT StorageName INTO v_patient_name FROM StorageMaster WHERE StorageId = p_storage_id;

    -- Create invoice
    INSERT INTO BillingTransaction (CenterId, InvoiceNumber, PatientId, TotalAmount,
        TaxAmount, Discount, PaymentStatus, PaymentMode, InvoiceDate, CreatedAt, CreatedBy)
    VALUES (p_center_id, v_inv_no, NULL, 0, 0, p_discount,
        CASE WHEN p_payment_mode = 'Paid' THEN 'Paid' ELSE 'Credit' END,
        p_payment_mode, p_issue_date, NOW(), p_created_by)
    RETURNING BillingTransactionId INTO v_invoice_id;

    -- Process each component
    FOREACH v_cid IN ARRAY p_component_ids
    LOOP
        SELECT c.componenttype, c.componentcode, c.bagid
        INTO v_component_type, v_component_code, v_bag_id
        FROM ComponentMaster c WHERE c.componentid = v_cid AND c.currentstatus = 'Available';

        IF NOT FOUND THEN CONTINUE; END IF;

        -- Get rate for this component type from storage
        v_rate := fn_issue_storage_get_storage_rate(p_storage_id, v_component_type);

        -- Create invoice detail
        INSERT INTO InvoiceDetail (BillingTransactionId, ComponentId, ServiceName,
            Quantity, UnitPrice, LineTotal)
        VALUES (v_invoice_id, v_cid,
            v_component_type || ' (' || v_component_code || ')',
            1, v_rate, v_rate);

        -- Update component status
        UPDATE ComponentMaster SET currentstatus = 'Issued' WHERE componentid = v_cid;
        UPDATE BloodBagMaster SET BagStatus = 'Issued', UpdatedAt = NOW() WHERE BagId = v_bag_id;

        -- Create IssueStorage record
        INSERT INTO IssueStorage (CenterId, StorageId, ComponentId, InvoiceId, IssueDateTime, Rate)
        VALUES (p_center_id, p_storage_id, v_cid, v_invoice_id, p_issue_date, v_rate);

        v_total := v_total + v_rate;
    END LOOP;

    -- Update invoice total
    UPDATE BillingTransaction SET TotalAmount = v_total + p_em_amt
    WHERE BillingTransactionId = v_invoice_id;

    RETURN v_invoice_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_issue_storage_get_by_center(
    p_center_id BIGINT,
    p_storage_id BIGINT DEFAULT NULL,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL
)
RETURNS TABLE(
    IssueStorageId BIGINT, StorageId BIGINT, StorageName VARCHAR,
    ComponentId BIGINT, ComponentCode VARCHAR, ComponentType VARCHAR,
    BloodGroup VARCHAR, BagNo VARCHAR, InvoiceId BIGINT, InvoiceNumber VARCHAR,
    IssueDateTime TIMESTAMPTZ, Rate NUMERIC
) AS $$
BEGIN
    RETURN QUERY SELECT i.IssueStorageId, s.StorageId, s.StorageName,
        c.componentid, c.componentcode, c.componenttype,
        d.bloodgroup, bg.bagno, b.BillingTransactionId, b.InvoiceNumber,
        i.IssueDateTime, i.Rate
    FROM IssueStorage i
    JOIN StorageMaster s ON s.StorageId = i.StorageId
    JOIN ComponentMaster c ON c.componentid = i.ComponentId
    JOIN BloodBagMaster bg ON bg.bagid = c.bagid
    JOIN DonorMaster d ON d.donorid = bg.donorid
    LEFT JOIN BillingTransaction b ON b.BillingTransactionId = i.InvoiceId
    WHERE i.CenterId = p_center_id
        AND (p_storage_id IS NULL OR i.StorageId = p_storage_id)
        AND (p_from_date IS NULL OR i.IssueDateTime::DATE >= p_from_date)
        AND (p_to_date IS NULL OR i.IssueDateTime::DATE <= p_to_date)
    ORDER BY i.IssueDateTime DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_issue_storage_get_invoices(
    p_center_id BIGINT,
    p_storage_id BIGINT DEFAULT NULL,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL
)
RETURNS TABLE(
    BillingTransactionId BIGINT, InvoiceNumber VARCHAR, IssueDateTime TIMESTAMPTZ,
    StorageName VARCHAR, TotalAmount NUMERIC, PaymentStatus VARCHAR, PaymentMode VARCHAR,
    Discount NUMERIC, DiscountReason VARCHAR, EmAmt NUMERIC, ComponentCount BIGINT
) AS $$
BEGIN
    RETURN QUERY SELECT b.BillingTransactionId, b.InvoiceNumber,
        i.IssueDateTime, s.StorageName,
        b.TotalAmount, b.PaymentStatus, b.PaymentMode,
        b.Discount, NULL::VARCHAR AS DiscountReason, 0::NUMERIC AS EmAmt,
        COUNT(DISTINCT i.ComponentId)::BIGINT AS ComponentCount
    FROM IssueStorage i
    JOIN StorageMaster s ON s.StorageId = i.StorageId
    JOIN BillingTransaction b ON b.BillingTransactionId = i.InvoiceId
    WHERE i.CenterId = p_center_id
        AND (p_storage_id IS NULL OR i.StorageId = p_storage_id)
        AND (p_from_date IS NULL OR i.IssueDateTime::DATE >= p_from_date)
        AND (p_to_date IS NULL OR i.IssueDateTime::DATE <= p_to_date)
    GROUP BY b.BillingTransactionId, b.InvoiceNumber, i.IssueDateTime,
        s.StorageName, b.TotalAmount, b.PaymentStatus, b.PaymentMode, b.Discount
    ORDER BY i.IssueDateTime DESC;
END;
$$ LANGUAGE plpgsql;
