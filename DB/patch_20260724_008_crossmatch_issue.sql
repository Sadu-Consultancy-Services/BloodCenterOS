-- ============================================================================
-- BloodCenterOS — Patch 20260724-008: Cross Matching + Blood Issuing
-- Description: Cross matching with Saline/Bovine/Coombs tests, and blood
--   issuing to patients with payment processing.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_008_crossmatch_issue.sql
-- ============================================================================

-- 1. CrossMatchEntry Table ---------------------------------------------------

CREATE TABLE IF NOT EXISTS CrossMatchEntry (
    CrossMatchEntryId BIGSERIAL PRIMARY KEY,
    CenterId          BIGINT NOT NULL DEFAULT 0,
    ReservationId     BIGINT NOT NULL REFERENCES PatientReservation(ReservationId),
    OverallResult     VARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pending, Pass, Reject
    Notes             VARCHAR(2000),
    PerformedBy       BIGINT,
    PerformedAt       TIMESTAMPTZ,
    CreatedAt         TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS CrossMatchTestResult (
    TestResultId      BIGSERIAL PRIMARY KEY,
    CrossMatchEntryId BIGINT NOT NULL REFERENCES CrossMatchEntry(CrossMatchEntryId) ON DELETE CASCADE,
    ReservationDetailId BIGINT NOT NULL REFERENCES ReservationDetail(ReservationDetailId),
    TestType          VARCHAR(50) NOT NULL,  -- Saline, Bovine, Coombs
    Result            VARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pass, Reject, Pending
    CreatedAt         TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Cross Match SPs --------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_crossmatch_start(
    p_center_id BIGINT,
    p_reservation_id BIGINT,
    p_performed_by BIGINT
) RETURNS BIGINT AS $$
DECLARE
    v_entry_id BIGINT;
    v_detail RECORD;
BEGIN
    -- Validate reservation is Active
    IF NOT EXISTS (SELECT 1 FROM PatientReservation WHERE ReservationId = p_reservation_id AND Status = 'Active' AND CenterId = p_center_id) THEN
        RAISE EXCEPTION 'Reservation is not active or not found';
    END IF;

    -- Create cross match entry
    INSERT INTO CrossMatchEntry (CenterId, ReservationId, OverallResult, PerformedBy, PerformedAt)
    VALUES (p_center_id, p_reservation_id, 'Pending', p_performed_by, NOW())
    RETURNING CrossMatchEntryId INTO v_entry_id;

    -- Create test result rows for each reserved component × 3 test types
    FOR v_detail IN SELECT ReservationDetailId FROM ReservationDetail
        WHERE ReservationId = p_reservation_id AND Status = 'Reserved'
    LOOP
        INSERT INTO CrossMatchTestResult (CrossMatchEntryId, ReservationDetailId, TestType, Result)
        VALUES (v_entry_id, v_detail.ReservationDetailId, 'Saline', 'Pending');

        INSERT INTO CrossMatchTestResult (CrossMatchEntryId, ReservationDetailId, TestType, Result)
        VALUES (v_entry_id, v_detail.ReservationDetailId, 'Bovine', 'Pending');

        INSERT INTO CrossMatchTestResult (CrossMatchEntryId, ReservationDetailId, TestType, Result)
        VALUES (v_entry_id, v_detail.ReservationDetailId, 'Coombs', 'Pending');
    END LOOP;

    RETURN v_entry_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_crossmatch_set_result(
    p_test_result_id BIGINT,
    p_result VARCHAR
) RETURNS VOID AS $$
DECLARE
    v_entry_id BIGINT;
    v_total INT;
    v_pass INT;
    v_reject INT;
BEGIN
    UPDATE CrossMatchTestResult SET Result = p_result
    WHERE TestResultId = p_test_result_id;

    -- Get the parent entry
    SELECT CrossMatchEntryId INTO v_entry_id FROM CrossMatchTestResult WHERE TestResultId = p_test_result_id;

    -- Recalculate overall result
    SELECT COUNT(*), SUM(CASE WHEN Result = 'Pass' THEN 1 ELSE 0 END), SUM(CASE WHEN Result = 'Reject' THEN 1 ELSE 0 END)
    INTO v_total, v_pass, v_reject
    FROM CrossMatchTestResult WHERE CrossMatchEntryId = v_entry_id;

    IF v_reject > 0 THEN
        UPDATE CrossMatchEntry SET OverallResult = 'Reject' WHERE CrossMatchEntryId = v_entry_id;
    ELSIF v_total > 0 AND v_pass = v_total THEN
        UPDATE CrossMatchEntry SET OverallResult = 'Pass' WHERE CrossMatchEntryId = v_entry_id;
    ELSE
        UPDATE CrossMatchEntry SET OverallResult = 'Pending' WHERE CrossMatchEntryId = v_entry_id;
    END IF;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_crossmatch_get_pending_reservations(p_center_id BIGINT)
RETURNS TABLE(
    ReservationId BIGINT, PatientName VARCHAR, RequiredBloodGroup VARCHAR,
    ComponentType VARCHAR, UnitsReserved INT, HospitalName VARCHAR, ReservationDate DATE
) AS $$
BEGIN
    RETURN QUERY SELECT r.ReservationId, r.PatientName, r.RequiredBloodGroup,
        r.ComponentType, r.UnitsReserved, r.HospitalName, r.ReservationDate
    FROM PatientReservation r
    WHERE r.CenterId = p_center_id AND r.Status = 'Active'
        AND r.UnitsReserved > 0
        AND NOT EXISTS (SELECT 1 FROM CrossMatchEntry e
            WHERE e.ReservationId = r.ReservationId AND e.OverallResult IN ('Pass', 'Reject'))
    ORDER BY r.CreatedAt;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_crossmatch_get_by_center(
    p_center_id BIGINT,
    p_status VARCHAR DEFAULT NULL,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL
)
RETURNS TABLE(
    CrossMatchEntryId BIGINT, ReservationId BIGINT, PatientName VARCHAR,
    RequiredBloodGroup VARCHAR, ComponentType VARCHAR, UnitsReserved INT,
    OverallResult VARCHAR, PerformedBy BIGINT, PerformedAt TIMESTAMPTZ, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT e.CrossMatchEntryId, r.ReservationId,
        r.PatientName, r.RequiredBloodGroup, r.ComponentType, r.UnitsReserved,
        e.OverallResult, e.PerformedBy, e.PerformedAt, e.CreatedAt
    FROM CrossMatchEntry e
    JOIN PatientReservation r ON r.ReservationId = e.ReservationId
    WHERE e.CenterId = p_center_id
        AND (p_status IS NULL OR e.OverallResult = p_status)
        AND (p_from_date IS NULL OR e.PerformedAt::DATE >= p_from_date)
        AND (p_to_date IS NULL OR e.PerformedAt::DATE <= p_to_date)
    ORDER BY e.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_crossmatch_get_by_id(p_entry_id BIGINT)
RETURNS TABLE(
    CrossMatchEntryId BIGINT, ReservationId BIGINT, OverallResult VARCHAR,
    Notes VARCHAR, PerformedBy BIGINT, PerformedAt TIMESTAMPTZ, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT e.CrossMatchEntryId, e.ReservationId, e.OverallResult,
        e.Notes, e.PerformedBy, e.PerformedAt, e.CreatedAt
    FROM CrossMatchEntry e WHERE e.CrossMatchEntryId = p_entry_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_crossmatch_get_tests(p_entry_id BIGINT)
RETURNS TABLE(
    TestResultId BIGINT, CrossMatchEntryId BIGINT, ReservationDetailId BIGINT,
    ComponentCode VARCHAR, BloodGroup VARCHAR, ComponentType VARCHAR, VolumeMl INT,
    TestType VARCHAR, Result VARCHAR, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT t.TestResultId, t.CrossMatchEntryId, t.ReservationDetailId,
        rd.ComponentCode, rd.BloodGroup, rd.ComponentType, rd.VolumeMl,
        t.TestType, t.Result, t.CreatedAt
    FROM CrossMatchTestResult t
    JOIN ReservationDetail rd ON rd.ReservationDetailId = t.ReservationDetailId
    WHERE t.CrossMatchEntryId = p_entry_id
    ORDER BY rd.ReservationDetailId, t.TestType;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_crossmatch_reject_component(
    p_test_result_id BIGINT
) RETURNS VOID AS $$
DECLARE
    v_entry_id BIGINT;
    v_reservation_id BIGINT;
    v_detail_id BIGINT;
BEGIN
    SELECT CrossMatchEntryId, ReservationDetailId INTO v_entry_id, v_detail_id
    FROM CrossMatchTestResult WHERE TestResultId = p_test_result_id;

    -- Mark all 3 tests for this component as Reject
    UPDATE CrossMatchTestResult SET Result = 'Reject'
    WHERE ReservationDetailId = v_detail_id AND CrossMatchEntryId = v_entry_id;

    -- Release the component back to Available
    SELECT ReservationId INTO v_reservation_id FROM ReservationDetail WHERE ReservationDetailId = v_detail_id;

    UPDATE ComponentMaster SET currentstatus = 'Available'
    WHERE componentid = (SELECT ComponentId FROM ReservationDetail WHERE ReservationDetailId = v_detail_id);

    UPDATE ReservationDetail SET Status = 'Released'
    WHERE ReservationDetailId = v_detail_id;

    -- Recalculate overall
    PERFORM fn_crossmatch_set_result(p_test_result_id, 'Reject');
END;
$$ LANGUAGE plpgsql;


-- 3. Blood Issue SPs ---------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_issue_from_reservation(
    p_center_id BIGINT,
    p_reservation_id BIGINT,
    p_issue_type VARCHAR DEFAULT 'Patient',
    p_payment_mode VARCHAR DEFAULT NULL,   -- Cash, Credit, or NULL (no invoice)
    p_issued_by BIGINT DEFAULT NULL,
    p_notes VARCHAR DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE
    v_invoice_id BIGINT;
    v_inv_no VARCHAR;
    v_issue_count INT := 0;
    v_detail RECORD;
    v_total_amount NUMERIC := 0;
    v_issue_id BIGINT;
BEGIN
    -- Validate: must have a passed cross-match
    IF NOT EXISTS (SELECT 1 FROM CrossMatchEntry e
        WHERE e.ReservationId = p_reservation_id AND e.OverallResult = 'Pass') THEN
        RAISE EXCEPTION 'Reservation has not passed cross-match';
    END IF;

    -- Process each reserved component
    FOR v_detail IN SELECT rd.ReservationDetailId, rd.ComponentId, rd.ComponentCode,
        rd.BloodGroup, rd.ComponentType, rd.VolumeMl, rd.UnitRate,
        b.BagId
        FROM ReservationDetail rd
        JOIN BloodBagMaster b ON b.DonorId IN (SELECT DonorId FROM ComponentMaster WHERE ComponentId = rd.ComponentId)
        WHERE rd.ReservationId = p_reservation_id AND rd.Status = 'Reserved'
    LOOP
        -- Update component status
        UPDATE ComponentMaster SET currentstatus = 'Issued'
        WHERE componentid = v_detail.ComponentId AND currentstatus = 'Reserved';

        -- Update detail status
        UPDATE ReservationDetail SET Status = 'Issued'
        WHERE ReservationDetailId = v_detail.ReservationDetailId;

        -- Create issue record
        INSERT INTO IssueRecord (CenterId, ComponentId, BagId, PatientName,
            IssueDate, IssuedByUserId, IssueType, Notes, RelatedBillingId)
        VALUES (p_center_id, v_detail.ComponentId, v_detail.BagId,
            (SELECT PatientName FROM PatientReservation WHERE ReservationId = p_reservation_id),
            NOW(), p_issued_by, p_issue_type, p_notes, NULL);

        v_issue_count := v_issue_count + 1;
        v_total_amount := v_total_amount + COALESCE(v_detail.UnitRate, 0);
    END LOOP;

    -- Update reservation status
    UPDATE PatientReservation SET Status = 'Issued'
    WHERE ReservationId = p_reservation_id;

    -- Create invoice if payment mode specified
    IF p_payment_mode IS NOT NULL AND v_total_amount > 0 THEN
        v_inv_no := 'INV-I-' || p_reservation_id || '-' || TO_CHAR(NOW(), 'YYYYMMDD');

        INSERT INTO BillingTransaction (CenterId, InvoiceNumber, PatientId, TotalAmount,
            PaymentStatus, PaymentMode, InvoiceDate, CreatedAt, CreatedBy)
        VALUES (p_center_id, v_inv_no, NULL, v_total_amount,
            CASE WHEN p_payment_mode = 'Cash' THEN 'Paid' ELSE 'Pending' END,
            p_payment_mode, NOW(), NOW(), p_issued_by)
        RETURNING BillingTransactionId INTO v_invoice_id;

        INSERT INTO InvoiceDetail (BillingTransactionId, ComponentId, ServiceName, Quantity, UnitPrice, LineTotal)
        SELECT v_invoice_id, rd.ComponentId, 'Blood - ' || rd.ComponentType || ' (' || rd.BloodGroup || ')',
            1, rd.UnitRate, rd.UnitRate
        FROM ReservationDetail rd WHERE rd.ReservationId = p_reservation_id AND rd.Status = 'Issued';

        -- Link invoice to reservation
        UPDATE PatientReservation SET InvoiceId = v_invoice_id
        WHERE ReservationId = p_reservation_id;
    END IF;

    RETURN v_issue_count;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_issue_get_by_reservation(p_reservation_id BIGINT)
RETURNS TABLE(
    IssueRecordId BIGINT, ComponentId BIGINT, ComponentCode VARCHAR,
    ComponentType VARCHAR, BloodGroup VARCHAR, PatientName VARCHAR,
    IssueDate TIMESTAMPTZ, IssueType VARCHAR, Notes VARCHAR
) AS $$
BEGIN
    RETURN QUERY SELECT i.IssueRecordId, i.ComponentId,
        c.componentcode, c.componenttype, d.bloodgroup,
        i.PatientName, i.IssueDate, i.IssueType, i.Notes
    FROM IssueRecord i
    JOIN ComponentMaster c ON c.componentid = i.ComponentId
    JOIN BloodBagMaster b ON b.bagid = i.BagId
    JOIN DonorMaster d ON d.donorid = b.DonorId
    WHERE i.RelatedBillingId IN (
        SELECT InvoiceId FROM PatientReservation WHERE ReservationId = p_reservation_id
    )
    ORDER BY i.IssueDate;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_issue_get_ready_for_issue(p_center_id BIGINT)
RETURNS TABLE(
    ReservationId BIGINT, PatientName VARCHAR, RequiredBloodGroup VARCHAR,
    ComponentType VARCHAR, UnitsReserved INT, HospitalName VARCHAR,
    CrossMatchEntryId BIGINT, OverallResult VARCHAR
) AS $$
BEGIN
    RETURN QUERY SELECT r.ReservationId, r.PatientName, r.RequiredBloodGroup,
        r.ComponentType, r.UnitsReserved, r.HospitalName,
        e.CrossMatchEntryId, e.OverallResult
    FROM PatientReservation r
    JOIN CrossMatchEntry e ON e.ReservationId = r.ReservationId
    WHERE r.CenterId = p_center_id AND r.Status = 'Active'
        AND e.OverallResult = 'Pass'
    ORDER BY r.CreatedAt;
END;
$$ LANGUAGE plpgsql;
