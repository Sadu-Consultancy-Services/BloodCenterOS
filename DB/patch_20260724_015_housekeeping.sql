-- ============================================================================
-- BloodCenterOS — Patch 20260724-015: Phase 10 Housekeeping
-- Description: Stored procedures for remaining features
--   PatientRequest, Expense list, Appointment list, Return list
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_015_housekeeping.sql
-- ============================================================================

-- 1. PatientRequest (already exists in v3.1 SPs, register here)
CREATE OR REPLACE FUNCTION fn_patient_request_create(
    p_center_id BIGINT, p_hospital_id BIGINT, p_patient_name VARCHAR,
    p_age INT, p_gender VARCHAR, p_blood_group VARCHAR, p_component_type VARCHAR,
    p_units INT, p_urgency VARCHAR, p_requested_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO PatientRequest (CenterId, HospitalId, PatientName, PatientAge,
        PatientGender, BloodGroup, ComponentType, UnitsRequested, RequestDate,
        RequestUrgency, RequestedByUserId)
    VALUES (p_center_id, p_hospital_id, p_patient_name, p_age, p_gender, p_blood_group,
        p_component_type, p_units, NOW(), p_urgency, p_requested_by)
    RETURNING RequestId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_patient_request_get_pending(p_center_id BIGINT)
RETURNS TABLE(RequestId BIGINT, PatientName VARCHAR, BloodGroup VARCHAR,
    ComponentType VARCHAR, UnitsRequested INT, RequestUrgency VARCHAR,
    RequestDate TIMESTAMPTZ, HospitalName VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT pr.RequestId, pr.PatientName, pr.BloodGroup, pr.ComponentType,
        pr.UnitsRequested, pr.RequestUrgency, pr.RequestDate, COALESCE(h.HospitalName, '')::VARCHAR
    FROM PatientRequest pr
    LEFT JOIN HospitalMaster h ON h.HospitalId = pr.HospitalId
    WHERE pr.CenterId = p_center_id
        AND pr.RelatedIssueId IS NULL
    ORDER BY pr.RequestUrgency = 'Emergency' DESC, pr.RequestDate;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_patient_request_get_all(p_center_id BIGINT)
RETURNS TABLE(RequestId BIGINT, PatientName VARCHAR, BloodGroup VARCHAR,
    ComponentType VARCHAR, UnitsRequested INT, RequestUrgency VARCHAR,
    RequestDate TIMESTAMPTZ, HospitalName VARCHAR, RelatedIssueId BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT pr.RequestId, pr.PatientName, pr.BloodGroup, pr.ComponentType,
        pr.UnitsRequested, pr.RequestUrgency, pr.RequestDate, COALESCE(h.HospitalName, '')::VARCHAR,
        pr.RelatedIssueId
    FROM PatientRequest pr
    LEFT JOIN HospitalMaster h ON h.HospitalId = pr.HospitalId
    WHERE pr.CenterId = p_center_id
    ORDER BY pr.RequestDate DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_patient_request_get_by_id(
    p_center_id BIGINT, p_request_id BIGINT
) RETURNS TABLE(RequestId BIGINT, CenterId BIGINT, HospitalId BIGINT,
    PatientName VARCHAR, PatientAge INT, PatientGender VARCHAR,
    BloodGroup VARCHAR, ComponentType VARCHAR, UnitsRequested INT,
    RequestDate TIMESTAMPTZ, RequestUrgency VARCHAR, RequestedByUserId BIGINT,
    RelatedIssueId BIGINT, HospitalName VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT pr.RequestId, pr.CenterId, pr.HospitalId,
        pr.PatientName, pr.PatientAge, pr.PatientGender, pr.BloodGroup,
        pr.ComponentType, pr.UnitsRequested, pr.RequestDate, pr.RequestUrgency,
        pr.RequestedByUserId, pr.RelatedIssueId, COALESCE(h.HospitalName, '')::VARCHAR
    FROM PatientRequest pr
    LEFT JOIN HospitalMaster h ON h.HospitalId = pr.HospitalId
    WHERE pr.RequestId = p_request_id AND pr.CenterId = p_center_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_patient_request_link_issue(
    p_center_id BIGINT, p_request_id BIGINT, p_issue_id BIGINT
) RETURNS VOID AS $$
BEGIN
    UPDATE PatientRequest SET RelatedIssueId = p_issue_id WHERE RequestId = p_request_id AND CenterId = p_center_id;
END;
$$ LANGUAGE plpgsql;

-- 2. Expense list
CREATE OR REPLACE FUNCTION fn_expense_get_all(
    p_center_id BIGINT, p_from_date TIMESTAMP DEFAULT NULL, p_to_date TIMESTAMP DEFAULT NULL
) RETURNS TABLE(ExpenseId BIGINT, ExpenseDate TIMESTAMPTZ, Category VARCHAR,
    Amount NUMERIC, Notes VARCHAR, CreatedBy BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT e.ExpenseId, e.ExpenseDate, e.Category::VARCHAR, e.Amount, e.Notes::VARCHAR, e.CreatedBy
    FROM ExpenseMaster e
    WHERE e.CenterId = p_center_id
        AND (p_from_date IS NULL OR e.ExpenseDate::DATE >= p_from_date::DATE)
        AND (p_to_date IS NULL OR e.ExpenseDate::DATE <= p_to_date::DATE)
    ORDER BY e.ExpenseDate DESC;
END;
$$ LANGUAGE plpgsql;

-- 3. Appointment list
CREATE OR REPLACE FUNCTION fn_appointment_get_all(
    p_center_id BIGINT, p_donor_id BIGINT DEFAULT NULL
) RETURNS TABLE(AppointmentId BIGINT, DonorId BIGINT, DonorName VARCHAR,
    AppointmentDate DATE, Slot VARCHAR, Status VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT a.AppointmentId, a.DonorId,
        COALESCE(d.FirstName || ' ' || COALESCE(d.LastName, ''), '')::VARCHAR AS DonorName,
        a.AppointmentDate, a.Slot::VARCHAR, a.Status::VARCHAR, a.CreatedAt
    FROM DonorAppointment a
    LEFT JOIN DonorMaster d ON d.DonorId = a.DonorId
    WHERE a.CenterId = p_center_id
        AND (p_donor_id IS NULL OR a.DonorId = p_donor_id)
    ORDER BY a.AppointmentDate DESC, a.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;

-- 4. Return list
CREATE OR REPLACE FUNCTION fn_return_get_all(p_center_id BIGINT)
RETURNS TABLE(ReturnId BIGINT, IssueRecordId BIGINT, ComponentId BIGINT,
    ComponentCode VARCHAR, ComponentType VARCHAR, PatientName VARCHAR,
    ReturnDate TIMESTAMPTZ, Reason VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT r.ReturnId, r.IssueRecordId, r.ComponentId,
        COALESCE(cm.ComponentCode, '')::VARCHAR,
        COALESCE(cm.ComponentType, '')::VARCHAR,
        COALESCE(ir.PatientName, '')::VARCHAR,
        r.ReturnDate, r.Reason::VARCHAR
    FROM ReturnRecord r
    LEFT JOIN IssueRecord ir ON ir.IssueRecordId = r.IssueRecordId
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = r.ComponentId
    WHERE r.CenterId = p_center_id
    ORDER BY r.ReturnDate DESC;
END;
$$ LANGUAGE plpgsql;
