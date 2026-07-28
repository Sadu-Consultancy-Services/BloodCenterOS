-- ============================================================================
-- BloodCenterOS — Patch 20260724-014: Reports (Phase 9)
-- Description: Stored procedures for all 20 Crystal Reports converted to
--   SP-based web reports.
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_014_reports.sql
-- ============================================================================

-- 1. Blood Stock Report (#6) — Current stock by blood group × component type
CREATE OR REPLACE FUNCTION fn_report_blood_stock(
    p_center_id BIGINT
) RETURNS TABLE(
    blood_group VARCHAR, component_type VARCHAR,
    available_qty BIGINT, expiry_date DATE
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COALESCE(d.BloodGroup, '')::VARCHAR AS blood_group,
        COALESCE(cm.ComponentType, '')::VARCHAR AS component_type,
        COUNT(cm.ComponentId)::BIGINT AS available_qty,
        MIN(cm.ExpiryDate)::DATE AS expiry_date
    FROM ComponentMaster cm
    LEFT JOIN BloodBagMaster bbm ON bbm.BagId = cm.BagId
    LEFT JOIN DonorMaster d ON d.DonorId = bbm.DonorId
    WHERE cm.CenterId = p_center_id
        AND cm.CurrentStatus = 'Available'
    GROUP BY d.BloodGroup, cm.ComponentType
    ORDER BY d.BloodGroup, cm.ComponentType;
END;
$$ LANGUAGE plpgsql;

-- 2. Procurement Summary Report (#5) — 8×4 matrix by date range
CREATE OR REPLACE FUNCTION fn_report_procurement_summary(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP
) RETURNS TABLE(
    blood_group VARCHAR,
    wb_available BIGINT, wb_issued BIGINT, wb_discarded BIGINT,
    pcv_available BIGINT, pcv_issued BIGINT, pcv_discarded BIGINT,
    ffp_available BIGINT, ffp_issued BIGINT, ffp_discarded BIGINT,
    pc_available BIGINT, pc_issued BIGINT, pc_discarded BIGINT,
    total_available BIGINT, total_issued BIGINT, total_discarded BIGINT
) AS $$
BEGIN
    RETURN QUERY
    WITH components AS (
        SELECT
            COALESCE(d.BloodGroup, 'Unknown') AS bg,
            cm.ComponentType AS ct,
            cm.CurrentStatus AS st
        FROM ComponentMaster cm
        LEFT JOIN BloodBagMaster bbm ON bbm.BagId = cm.BagId
        LEFT JOIN DonorMaster d ON d.DonorId = bbm.DonorId
        WHERE cm.CenterId = p_center_id
            AND cm.CreatedAt::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    )
    SELECT
        bg AS blood_group,
        COUNT(*) FILTER (WHERE ct IN ('WB','SB') AND st = 'Available')::BIGINT AS wb_available,
        COUNT(*) FILTER (WHERE ct IN ('WB','SB') AND st = 'Issued')::BIGINT AS wb_issued,
        COUNT(*) FILTER (WHERE ct IN ('WB','SB') AND st = 'Discarded')::BIGINT AS wb_discarded,
        COUNT(*) FILTER (WHERE ct IN ('PCV') AND st = 'Available')::BIGINT AS pcv_available,
        COUNT(*) FILTER (WHERE ct IN ('PCV') AND st = 'Issued')::BIGINT AS pcv_issued,
        COUNT(*) FILTER (WHERE ct IN ('PCV') AND st = 'Discarded')::BIGINT AS pcv_discarded,
        COUNT(*) FILTER (WHERE ct IN ('FFP') AND st = 'Available')::BIGINT AS ffp_available,
        COUNT(*) FILTER (WHERE ct IN ('FFP') AND st = 'Issued')::BIGINT AS ffp_issued,
        COUNT(*) FILTER (WHERE ct IN ('FFP') AND st = 'Discarded')::BIGINT AS ffp_discarded,
        COUNT(*) FILTER (WHERE ct IN ('PC','PltsConc') AND st = 'Available')::BIGINT AS pc_available,
        COUNT(*) FILTER (WHERE ct IN ('PC','PltsConc') AND st = 'Issued')::BIGINT AS pc_issued,
        COUNT(*) FILTER (WHERE ct IN ('PC','PltsConc') AND st = 'Discarded')::BIGINT AS pc_discarded,
        COUNT(*) FILTER (WHERE st = 'Available')::BIGINT AS total_available,
        COUNT(*) FILTER (WHERE st = 'Issued')::BIGINT AS total_issued,
        COUNT(*) FILTER (WHERE st = 'Discarded')::BIGINT AS total_discarded
    FROM components
    GROUP BY bg
    ORDER BY bg;
END;
$$ LANGUAGE plpgsql;

-- 3. Donor List Report (#8) — Donors by date range
CREATE OR REPLACE FUNCTION fn_report_donor_list(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP,
    p_show_contact BOOLEAN DEFAULT TRUE
) RETURNS TABLE(
    donor_id BIGINT, donor_name VARCHAR, gender VARCHAR,
    blood_group VARCHAR, phone VARCHAR, email VARCHAR,
    last_donation_date DATE, total_donations BIGINT,
    created_at TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        d.DonorId,
        (COALESCE(d.FirstName,'') || ' ' || COALESCE(d.LastName,''))::VARCHAR AS donor_name,
        d.Gender::VARCHAR,
        d.BloodGroup::VARCHAR,
        CASE WHEN p_show_contact THEN d.Phone ELSE NULL END::VARCHAR AS phone,
        CASE WHEN p_show_contact THEN d.Email ELSE NULL END::VARCHAR AS email,
        d.LastDonationDate,
        d.TotalDonations::BIGINT,
        d.CreatedAt
    FROM DonorMaster d
    WHERE d.CenterId = p_center_id
        AND d.CreatedAt::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    ORDER BY d.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;

-- 4. Cross Match Income Report (#11) — Income by date range
CREATE OR REPLACE FUNCTION fn_report_cm_income(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP
) RETURNS TABLE(
    invoice_date DATE, invoice_id BIGINT,
    patient_name VARCHAR, total_amount NUMERIC,
    emergency_amount NUMERIC, discount NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        bt.InvoiceDate::DATE,
        bt.BillingTransactionId AS invoice_id,
        COALESCE(pr.PatientName, 'N/A')::VARCHAR AS patient_name,
        bt.TotalAmount,
        COALESCE(bt.TaxAmount, 0) AS emergency_amount,
        COALESCE(bt.Discount, 0) AS discount
    FROM BillingTransaction bt
    LEFT JOIN PatientReservation pr ON pr.InvoiceId = bt.BillingTransactionId
    WHERE bt.CenterId = p_center_id
        AND bt.InvoiceDate::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    ORDER BY bt.InvoiceDate DESC;
END;
$$ LANGUAGE plpgsql;

-- 5. Discount Details Report (#13) — Discount analysis by date range
CREATE OR REPLACE FUNCTION fn_report_discount_details(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP
) RETURNS TABLE(
    invoice_id BIGINT, invoice_date DATE,
    patient_name VARCHAR, gross_amount NUMERIC,
    discount_amount NUMERIC, net_amount NUMERIC,
    discount_reason VARCHAR, payment_status VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        bt.BillingTransactionId AS invoice_id,
        bt.InvoiceDate::DATE,
        COALESCE(pr.PatientName, 'N/A')::VARCHAR AS patient_name,
        bt.TotalAmount + COALESCE(bt.Discount, 0) AS gross_amount,
        COALESCE(bt.Discount, 0) AS discount_amount,
        bt.TotalAmount AS net_amount,
        ''::VARCHAR AS discount_reason,
        bt.PaymentStatus::VARCHAR
    FROM BillingTransaction bt
    LEFT JOIN PatientReservation pr ON pr.InvoiceId = bt.BillingTransactionId
    WHERE bt.CenterId = p_center_id
        AND bt.InvoiceDate::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
        AND (bt.Discount IS NULL OR bt.Discount > 0)
    ORDER BY bt.InvoiceDate DESC;
END;
$$ LANGUAGE plpgsql;

-- 6. Daily Issues Report (#14) — Component-wise issue counts by date range
CREATE OR REPLACE FUNCTION fn_report_daily_issues(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP
) RETURNS TABLE(
    issue_date DATE, invoice_id BIGINT,
    patient_name VARCHAR, component_type VARCHAR,
    quantity BIGINT, unit_price NUMERIC,
    line_total NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        bt.InvoiceDate::DATE AS issue_date,
        bt.BillingTransactionId AS invoice_id,
        COALESCE(pr.PatientName, 'N/A')::VARCHAR AS patient_name,
        COALESCE(idtl.ServiceName, cm.ComponentType, '')::VARCHAR AS component_type,
        COUNT(ic.ComponentId)::BIGINT AS quantity,
        COALESCE(AVG(idtl.UnitPrice), 0) AS unit_price,
        COALESCE(SUM(idtl.LineTotal), 0) AS line_total
    FROM BillingTransaction bt
    JOIN InvoiceDetail idtl ON idtl.BillingTransactionId = bt.BillingTransactionId
    LEFT JOIN PatientReservation pr ON pr.InvoiceId = bt.BillingTransactionId
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = idtl.ComponentId
    WHERE bt.CenterId = p_center_id
        AND bt.InvoiceDate::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    GROUP BY bt.InvoiceDate::DATE, bt.BillingTransactionId, pr.PatientName,
        COALESCE(idtl.ServiceName, cm.ComponentType, '')
    ORDER BY bt.InvoiceDate DESC, bt.BillingTransactionId;
END;
$$ LANGUAGE plpgsql;

-- 7. MBB Inward Report (#15) — MBB inward by date range
CREATE OR REPLACE FUNCTION fn_report_mbb_inward(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP,
    p_supplier_name VARCHAR DEFAULT NULL
) RETURNS TABLE(
    bill_id BIGINT, bill_number VARCHAR, bill_date TIMESTAMPTZ,
    supplier_name VARCHAR, component_type VARCHAR,
    blood_group VARCHAR, quantity BIGINT,
    unit_price NUMERIC, line_total NUMERIC,
    total_amount NUMERIC, payment_status VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        mb.MbbBillId AS bill_id,
        mb.BillNumber::VARCHAR,
        mb.BillDate,
        mb.SupplierName::VARCHAR,
        COALESCE(mbd.ComponentType, '')::VARCHAR,
        COALESCE(mbd.BloodGroup, '')::VARCHAR,
        mbd.Quantity::BIGINT,
        mbd.UnitPrice,
        (mbd.Quantity * mbd.UnitPrice) AS line_total,
        mb.TotalAmount,
        mb.PaymentStatus::VARCHAR
    FROM MbbBill mb
    JOIN MbbBillDetail mbd ON mbd.MbbBillId = mb.MbbBillId
    WHERE mb.CenterId = p_center_id
        AND mb.BillDate::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
        AND (p_supplier_name IS NULL OR mb.SupplierName ILIKE '%' || p_supplier_name || '%')
    ORDER BY mb.BillDate DESC, mb.MbbBillId;
END;
$$ LANGUAGE plpgsql;

-- 8. QC Daily Report (#18) — QC records by date
CREATE OR REPLACE FUNCTION fn_report_qc_daily(
    p_center_id BIGINT, p_qc_date TIMESTAMP
) RETURNS TABLE(
    qc_record_id BIGINT, qc_type VARCHAR, qc_date TIMESTAMPTZ,
    performed_by BIGINT, unit_number VARCHAR,
    specificity VARCHAR, batch_no VARCHAR,
    expiry DATE, reactivity VARCHAR,
    activity VARCHAR, titre VARCHAR,
    appearance VARCHAR, haemolysis VARCHAR,
    sp_gravity VARCHAR, high_control VARCHAR,
    low_control VARCHAR, notes VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        q.QCRecordId,
        q.QCType::VARCHAR,
        q.QCDate,
        q.PerformedBy,
        q.UnitNumber::VARCHAR,
        q.Specificity::VARCHAR,
        q.BatchNo::VARCHAR,
        q.Expiry,
        q.Reactivity::VARCHAR,
        q.Activity::VARCHAR,
        q.Titre::VARCHAR,
        q.Appearance::VARCHAR,
        q.Haemolysis::VARCHAR,
        q.SpGravity::VARCHAR,
        q.HighControl::VARCHAR,
        q.LowControl::VARCHAR,
        q.Notes::VARCHAR
    FROM QCRegister q
    WHERE q.CenterId = p_center_id
        AND q.QCDate::DATE = p_qc_date::DATE
    ORDER BY q.QCType, q.QCRecordId;
END;
$$ LANGUAGE plpgsql;

-- 9. Inventory Stock Report (#19) — Current inventory stock
CREATE OR REPLACE FUNCTION fn_report_inv_stock(
    p_center_id BIGINT
) RETURNS TABLE(
    item_id BIGINT, item_name VARCHAR, item_unit VARCHAR,
    min_order_qty BIGINT, current_stock BIGINT,
    last_transaction_date TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        i.ItemId,
        i.ItemName::VARCHAR,
        COALESCE(i.ItemUnit, '')::VARCHAR,
        i.MinOrderQty::BIGINT,
        COALESCE(SUM(CASE WHEN t.TransTyp = 'I' THEN t.TransQty ELSE 0 END)
            - SUM(CASE WHEN t.TransTyp = 'O' THEN t.TransQty ELSE 0 END), 0)::BIGINT AS current_stock,
        MAX(t.TransDate) AS last_transaction_date
    FROM InvItems i
    LEFT JOIN InvTrans t ON t.ItemId = i.ItemId AND t.CenterId = i.CenterId
    WHERE i.CenterId = p_center_id AND i.IsActive = TRUE
    GROUP BY i.ItemId, i.ItemName, i.ItemUnit, i.MinOrderQty
    ORDER BY i.ItemName;
END;
$$ LANGUAGE plpgsql;

-- 10. Inventory Inward/Outward Report (#20) — Transactions by date range
CREATE OR REPLACE FUNCTION fn_report_inv_inout(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP,
    p_trans_type VARCHAR DEFAULT NULL,
    p_item_ids BIGINT[] DEFAULT NULL
) RETURNS TABLE(
    trans_id BIGINT, item_name VARCHAR, trans_qty BIGINT,
    trans_typ VARCHAR, trans_date TIMESTAMPTZ,
    trans_desc VARCHAR, item_unit VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        t.TransId,
        i.ItemName::VARCHAR,
        t.TransQty::BIGINT,
        t.TransTyp::VARCHAR,
        t.TransDate,
        COALESCE(t.TransDesc, '')::VARCHAR,
        COALESCE(i.ItemUnit, '')::VARCHAR
    FROM InvTrans t
    JOIN InvItems i ON i.ItemId = t.ItemId
    WHERE t.CenterId = p_center_id
        AND t.TransDate::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
        AND (p_trans_type IS NULL OR t.TransTyp = p_trans_type)
        AND (p_item_ids IS NULL OR t.ItemId = ANY(p_item_ids))
    ORDER BY t.TransDate DESC, t.TransId;
END;
$$ LANGUAGE plpgsql;

-- 11. Invoice Detail Report (#1, #2, #3, #10) — Single invoice with all details
CREATE OR REPLACE FUNCTION fn_report_invoice_detail(
    p_center_id BIGINT, p_invoice_id BIGINT
) RETURNS TABLE(
    invoice_id BIGINT, invoice_date TIMESTAMPTZ,
    patient_name VARCHAR, patient_address VARCHAR,
    patient_contact VARCHAR, patient_blood_group VARCHAR,
    hospital_name VARCHAR, ward VARCHAR,
    total_amount NUMERIC, discount NUMERIC,
    tax_amount NUMERIC, payment_status VARCHAR,
    payment_mode VARCHAR, component_code VARCHAR,
    component_type VARCHAR, blood_group VARCHAR,
    quantity BIGINT, unit_price NUMERIC, line_total NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        bt.BillingTransactionId AS invoice_id,
        bt.InvoiceDate,
        COALESCE(pr.PatientName, 'N/A')::VARCHAR AS patient_name,
        COALESCE(pr.PatientAddress, '')::VARCHAR,
        COALESCE(pr.PatientContactNo, '')::VARCHAR,
        COALESCE(pr.PatientBloodGroup, '')::VARCHAR,
        COALESCE(pr.HospitalName, '')::VARCHAR,
        COALESCE(pr.Ward, '')::VARCHAR,
        bt.TotalAmount,
        COALESCE(bt.Discount, 0),
        COALESCE(bt.TaxAmount, 0),
        bt.PaymentStatus::VARCHAR,
        COALESCE(bt.PaymentMode, '')::VARCHAR,
        COALESCE(cm.ComponentCode, '')::VARCHAR,
        COALESCE(idtl.ServiceName, cm.ComponentType, '')::VARCHAR AS component_type,
        COALESCE(pr.RequiredBloodGroup, '')::VARCHAR AS blood_group,
        idtl.Quantity::BIGINT,
        idtl.UnitPrice,
        idtl.LineTotal
    FROM BillingTransaction bt
    JOIN InvoiceDetail idtl ON idtl.BillingTransactionId = bt.BillingTransactionId
    LEFT JOIN PatientReservation pr ON pr.InvoiceId = bt.BillingTransactionId
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = idtl.ComponentId
    WHERE bt.BillingTransactionId = p_invoice_id
        AND bt.CenterId = p_center_id
    ORDER BY idtl.InvoiceDetailId;
END;
$$ LANGUAGE plpgsql;

-- 12. Blood Storage Invoice Detail Report (#4)
CREATE OR REPLACE FUNCTION fn_report_bs_invoice_detail(
    p_center_id BIGINT, p_invoice_id BIGINT
) RETURNS TABLE(
    invoice_id BIGINT, invoice_date TIMESTAMPTZ,
    storage_name VARCHAR, storage_address VARCHAR,
    component_code VARCHAR, component_type VARCHAR,
    blood_group VARCHAR, donor_name VARCHAR,
    donation_date DATE, expiry_date DATE,
    quantity BIGINT, unit_rate NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        bt.BillingTransactionId AS invoice_id,
        bt.InvoiceDate,
        COALESCE(s.StorageName, '')::VARCHAR,
        COALESCE(s.Address, '')::VARCHAR,
        COALESCE(cm.ComponentCode, '')::VARCHAR,
        COALESCE(cm.ComponentType, '')::VARCHAR,
        COALESCE(d.BloodGroup, '')::VARCHAR,
        COALESCE(d.FirstName || ' ' || COALESCE(d.LastName, ''), '')::VARCHAR AS donor_name,
        bbm.InitialCollectedAt::DATE AS donation_date,
        cm.ExpiryDate,
        1::BIGINT AS quantity,
        COALESCE(is2.Rate, 0) AS unit_rate
    FROM BillingTransaction bt
    JOIN IssueStorage is2 ON is2.InvoiceId = bt.BillingTransactionId
    LEFT JOIN StorageMaster s ON s.StorageId = is2.StorageId
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = is2.ComponentId
    LEFT JOIN BloodBagMaster bbm ON bbm.BagId = cm.BagId
    LEFT JOIN DonorMaster d ON d.DonorId = bbm.DonorId
    WHERE bt.BillingTransactionId = p_invoice_id
        AND bt.CenterId = p_center_id
    ORDER BY cm.ComponentCode;
END;
$$ LANGUAGE plpgsql;

-- 13. Cross Match Report (#10) — Individual CM by invoice
CREATE OR REPLACE FUNCTION fn_report_crossmatch(
    p_center_id BIGINT, p_invoice_id BIGINT
) RETURNS TABLE(
    invoice_id BIGINT, patient_name VARCHAR,
    patient_address VARCHAR, patient_blood_group VARCHAR,
    hospital_name VARCHAR, ward VARCHAR,
    reservation_id BIGINT, component_code VARCHAR,
    component_type VARCHAR, blood_group VARCHAR,
    overall_result VARCHAR, test_type VARCHAR,
    test_result VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        bt.BillingTransactionId AS invoice_id,
        COALESCE(pr.PatientName, 'N/A')::VARCHAR,
        COALESCE(pr.PatientAddress, '')::VARCHAR,
        COALESCE(pr.PatientBloodGroup, '')::VARCHAR,
        COALESCE(pr.HospitalName, '')::VARCHAR,
        COALESCE(pr.Ward, '')::VARCHAR,
        pr.ReservationId,
        COALESCE(cm.ComponentCode, '')::VARCHAR,
        COALESCE(cm.ComponentType, '')::VARCHAR,
        COALESCE(d.BloodGroup, '')::VARCHAR,
        ce.OverallResult::VARCHAR,
        ctr.TestType::VARCHAR,
        ctr.Result::VARCHAR
    FROM BillingTransaction bt
    JOIN PatientReservation pr ON pr.InvoiceId = bt.BillingTransactionId
    JOIN CrossMatchEntry ce ON ce.ReservationId = pr.ReservationId
    JOIN CrossMatchTestResult ctr ON ctr.CrossMatchEntryId = ce.CrossMatchEntryId
    LEFT JOIN ReservationDetail rd ON rd.ReservationDetailId = ctr.ReservationDetailId
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = rd.ComponentId
    LEFT JOIN BloodBagMaster bbm ON bbm.BagId = cm.BagId
    LEFT JOIN DonorMaster d ON d.DonorId = bbm.DonorId
    WHERE bt.BillingTransactionId = p_invoice_id
        AND bt.CenterId = p_center_id
    ORDER BY ctr.TestResultId;
END;
$$ LANGUAGE plpgsql;

-- 14. Discard Register Report (#12) — Discard records by date range
CREATE OR REPLACE FUNCTION fn_report_discard_register(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP,
    p_reason VARCHAR DEFAULT NULL
) RETURNS TABLE(
    discard_id BIGINT, component_code VARCHAR,
    component_type VARCHAR, donor_name VARCHAR,
    blood_group VARCHAR, discard_reason VARCHAR,
    discarded_at TIMESTAMPTZ, bag_number VARCHAR,
    autoclave_start TIMESTAMPTZ, autoclave_end TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        dr.DiscardId,
        COALESCE(cm.ComponentCode, '')::VARCHAR,
        COALESCE(cm.ComponentType, '')::VARCHAR,
        COALESCE(d.FirstName || ' ' || COALESCE(d.LastName, ''), '')::VARCHAR,
        COALESCE(d.BloodGroup, '')::VARCHAR,
        dr.DiscardReason::VARCHAR,
        dr.DiscardedAt,
        COALESCE(bbm.BloodBagNumber, '')::VARCHAR,
        dr.AutoClaveStartTime,
        dr.AutoClaveEndTime
    FROM DiscardRecord dr
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = dr.ComponentId
    LEFT JOIN BloodBagMaster bbm ON bbm.BagId = cm.BagId
    LEFT JOIN DonorMaster d ON d.DonorId = bbm.DonorId
    WHERE dr.CenterId = p_center_id
        AND dr.DiscardedAt::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
        AND (p_reason IS NULL OR dr.DiscardReason ILIKE '%' || p_reason || '%')
    ORDER BY dr.DiscardedAt DESC;
END;
$$ LANGUAGE plpgsql;

-- 15. Dues Register Report (#16) — Credit invoices with balances
CREATE OR REPLACE FUNCTION fn_report_dues_register(
    p_center_id BIGINT, p_as_on_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) RETURNS TABLE(
    invoice_id BIGINT, invoice_date DATE,
    patient_name VARCHAR, total_amount NUMERIC,
    paid_amount NUMERIC, due_amount NUMERIC,
    payment_status VARCHAR, days_overdue BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        bt.BillingTransactionId,
        bt.InvoiceDate::DATE,
        COALESCE(pr.PatientName, 'N/A')::VARCHAR,
        bt.TotalAmount,
        COALESCE(SUM(pm.Amount), 0) AS paid_amount,
        bt.TotalAmount - COALESCE(SUM(pm.Amount), 0) AS due_amount,
        bt.PaymentStatus::VARCHAR,
        (p_as_on_date::DATE - bt.InvoiceDate::DATE)::BIGINT AS days_overdue
    FROM BillingTransaction bt
    LEFT JOIN PatientReservation pr ON pr.InvoiceId = bt.BillingTransactionId
    LEFT JOIN PaymentRecord pm ON pm.BillingTransactionId = bt.BillingTransactionId
    WHERE bt.CenterId = p_center_id
        AND (bt.PaymentStatus = 'Credit' OR bt.PaymentStatus = 'Partial')
    GROUP BY bt.BillingTransactionId, bt.InvoiceDate, pr.PatientName,
        bt.TotalAmount, bt.PaymentStatus
    HAVING bt.TotalAmount - COALESCE(SUM(pm.Amount), 0) > 0
    ORDER BY bt.InvoiceDate;
END;
$$ LANGUAGE plpgsql;

-- 16. Autoclave Register Report (#17) — Autoclave records by date range
CREATE OR REPLACE FUNCTION fn_report_autoclave_register(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP
) RETURNS TABLE(
    discard_id BIGINT, component_code VARCHAR,
    component_type VARCHAR, donor_name VARCHAR,
    blood_group VARCHAR, discard_reason VARCHAR,
    autoclave_start TIMESTAMPTZ, autoclave_end TIMESTAMPTZ,
    bag_number VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        dr.DiscardId,
        COALESCE(cm.ComponentCode, '')::VARCHAR,
        COALESCE(cm.ComponentType, '')::VARCHAR,
        COALESCE(d.FirstName || ' ' || COALESCE(d.LastName, ''), '')::VARCHAR,
        COALESCE(d.BloodGroup, '')::VARCHAR,
        dr.DiscardReason::VARCHAR,
        dr.AutoClaveStartTime,
        dr.AutoClaveEndTime,
        COALESCE(bbm.BloodBagNumber, '')::VARCHAR
    FROM DiscardRecord dr
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = dr.ComponentId
    LEFT JOIN BloodBagMaster bbm ON bbm.BagId = cm.BagId
    LEFT JOIN DonorMaster d ON d.DonorId = bbm.DonorId
    WHERE dr.CenterId = p_center_id
        AND dr.AutoClaveStartTime IS NOT NULL
        AND dr.DiscardedAt::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    ORDER BY dr.AutoClaveStartTime DESC;
END;
$$ LANGUAGE plpgsql;
