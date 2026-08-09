-- Regen (v3): DROP + CREATE after full id rename
BEGIN;
DROP FUNCTION IF EXISTS public.fn_crossmatch_get_by_center(p_center_id bigint, p_status character varying, p_from_date date, p_to_date date) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_crossmatch_get_by_center(p_center_id bigint, p_status character varying DEFAULT NULL::character varying, p_from_date date DEFAULT NULL::date, p_to_date date DEFAULT NULL::date)
 RETURNS TABLE(crossmatchentryid bigint, BloodRequestId bigint, patientname character varying, requiredbloodgroup character varying, componenttype character varying, unitsreserved integer, overallresult character varying, performedby bigint, performedat timestamp with time zone, createdat timestamp with time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT e.CrossMatchEntryId, r.BloodRequestId,
        r.PatientName, r.RequiredBloodGroup, r.ComponentType, r.UnitsReserved,
        e.OverallResult, e.PerformedBy, e.PerformedAt, e.CreatedAt
    FROM CrossMatchEntry e
    JOIN BloodRequest r ON r.BloodRequestId = e.BloodRequestId
    WHERE e.CenterId = p_center_id
        AND (p_status IS NULL OR e.OverallResult = p_status)
        AND (p_from_date IS NULL OR e.PerformedAt::DATE >= p_from_date)
        AND (p_to_date IS NULL OR e.PerformedAt::DATE <= p_to_date)
    ORDER BY e.CreatedAt DESC;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_crossmatch_get_by_id(p_entry_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_crossmatch_get_by_id(p_entry_id bigint)
 RETURNS TABLE(crossmatchentryid bigint, BloodRequestId bigint, overallresult character varying, notes character varying, performedby bigint, performedat timestamp with time zone, createdat timestamp with time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT e.CrossMatchEntryId, e.BloodRequestId, e.OverallResult,
        e.Notes, e.PerformedBy, e.PerformedAt, e.CreatedAt
    FROM CrossMatchEntry e WHERE e.CrossMatchEntryId = p_entry_id;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_crossmatch_get_pending_reservations(p_center_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_crossmatch_get_pending_reservations(p_center_id bigint)
 RETURNS TABLE(BloodRequestId bigint, patientname character varying, requiredbloodgroup character varying, componenttype character varying, unitsreserved integer, hospitalname character varying, reservationdate date)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT r.BloodRequestId, r.PatientName, r.RequiredBloodGroup,
        r.ComponentType, r.UnitsReserved, r.HospitalName, r.ReservationDate
    FROM BloodRequest r
    WHERE r.CenterId = p_center_id AND r.Status = 'Active'
        AND r.UnitsReserved > 0
        AND NOT EXISTS (SELECT 1 FROM CrossMatchEntry e
            WHERE e.BloodRequestId = r.BloodRequestId AND e.OverallResult IN ('Pass', 'Reject'))
    ORDER BY r.CreatedAt;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_crossmatch_get_tests(p_entry_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_crossmatch_get_tests(p_entry_id bigint)
 RETURNS TABLE(testresultid bigint, crossmatchentryid bigint, BloodRequestDetailId bigint, componentcode character varying, bloodgroup character varying, componenttype character varying, volumeml integer, testtype character varying, result character varying, createdat timestamp with time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT t.TestResultId, t.CrossMatchEntryId, t.BloodRequestDetailId,
        rd.ComponentCode, rd.BloodGroup, rd.ComponentType, rd.VolumeMl,
        t.TestType, t.Result, t.CreatedAt
    FROM CrossMatchTestResult t
    JOIN BloodRequestDetail rd ON rd.BloodRequestDetailId = t.BloodRequestDetailId
    WHERE t.CrossMatchEntryId = p_entry_id
    ORDER BY rd.BloodRequestDetailId, t.TestType;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_crossmatch_reject_component(p_test_result_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_crossmatch_reject_component(p_test_result_id bigint)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_entry_id BIGINT;
    v_reservation_id BIGINT;
    v_detail_id BIGINT;
BEGIN
    SELECT CrossMatchEntryId, BloodRequestDetailId INTO v_entry_id, v_detail_id
    FROM CrossMatchTestResult WHERE TestResultId = p_test_result_id;

    -- Mark all 3 tests for this component as Reject
    UPDATE CrossMatchTestResult SET Result = 'Reject'
    WHERE BloodRequestDetailId = v_detail_id AND CrossMatchEntryId = v_entry_id;

    -- Release the component back to Available
    SELECT BloodRequestId INTO v_reservation_id FROM BloodRequestDetail WHERE BloodRequestDetailId = v_detail_id;

    UPDATE ComponentMaster SET currentstatus = 'Available'
    WHERE componentid = (SELECT ComponentId FROM BloodRequestDetail WHERE BloodRequestDetailId = v_detail_id);

    UPDATE BloodRequestDetail SET Status = 'Released'
    WHERE BloodRequestDetailId = v_detail_id;

    -- Recalculate overall
    PERFORM fn_crossmatch_set_result(p_test_result_id, 'Reject');
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_crossmatch_start(p_center_id bigint, p_reservation_id bigint, p_performed_by bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_crossmatch_start(p_center_id bigint, p_reservation_id bigint, p_performed_by bigint)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_entry_id BIGINT;
    v_detail RECORD;
BEGIN
    -- Validate reservation is Active
    IF NOT EXISTS (SELECT 1 FROM BloodRequest WHERE BloodRequestId = p_reservation_id AND Status = 'Active' AND CenterId = p_center_id) THEN
        RAISE EXCEPTION 'Reservation is not active or not found';
    END IF;

    -- Create cross match entry
    INSERT INTO CrossMatchEntry (CenterId, BloodRequestId, OverallResult, PerformedBy, PerformedAt)
    VALUES (p_center_id, p_reservation_id, 'Pending', p_performed_by, NOW())
    RETURNING CrossMatchEntryId INTO v_entry_id;

    -- Create test result rows for each reserved component x 3 test types
    FOR v_detail IN SELECT BloodRequestDetailId FROM BloodRequestDetail
        WHERE BloodRequestId = p_reservation_id AND Status = 'Reserved'
    LOOP
        INSERT INTO CrossMatchTestResult (CrossMatchEntryId, BloodRequestDetailId, TestType, Result)
        VALUES (v_entry_id, v_detail.BloodRequestDetailId, 'Saline', 'Pending');

        INSERT INTO CrossMatchTestResult (CrossMatchEntryId, BloodRequestDetailId, TestType, Result)
        VALUES (v_entry_id, v_detail.BloodRequestDetailId, 'Bovine', 'Pending');

        INSERT INTO CrossMatchTestResult (CrossMatchEntryId, BloodRequestDetailId, TestType, Result)
        VALUES (v_entry_id, v_detail.BloodRequestDetailId, 'Coombs', 'Pending');
    END LOOP;

    RETURN v_entry_id;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_emergency_request_create(p_center_id bigint, p_hospital_id bigint, p_patient_name character varying, p_blood_group character varying, p_component_type character varying, p_units integer, p_requested_by bigint, p_notes character varying) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_emergency_request_create(p_center_id bigint, p_hospital_id bigint, p_patient_name character varying, p_blood_group character varying, p_component_type character varying, p_units integer, p_requested_by bigint, p_notes character varying)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BloodRequest (CenterId, RequestType, HospitalId, PatientName,
        PatientBloodGroup, RequiredBloodGroup, ComponentType, UnitsRequested,
        RequestUrgency, RequestedByUserId, Status, Notes, ReservationDate, CreatedAt)
    VALUES (p_center_id, 'Emergency', p_hospital_id, p_patient_name,
        p_blood_group, p_blood_group, p_component_type, p_units,
        'Emergency', p_requested_by, 'Pending', p_notes, CURRENT_DATE, NOW())
    RETURNING BloodRequestId INTO v_id;
    RETURN v_id;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_emergency_request_get_pending(p_center_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_emergency_request_get_pending(p_center_id bigint)
 RETURNS TABLE(emergencyrequestid bigint, centerid bigint, hospitalid bigint, patientname character varying, bloodgroup character varying, componenttype character varying, unitsrequired integer, requeststatus character varying, requestedat timestamp with time zone, notes character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.BloodRequestId AS EmergencyRequestId, br.CenterId, br.HospitalId,
            br.PatientName, br.PatientBloodGroup AS BloodGroup, br.ComponentType,
            br.UnitsRequested AS UnitsRequired, br.Status AS RequestStatus,
            br.CreatedAt AS RequestedAt, br.Notes
        FROM BloodRequest br
        WHERE br.CenterId = p_center_id AND br.RequestType = 'Emergency'
            AND br.Status IN ('Pending','Processing')
        ORDER BY br.CreatedAt DESC;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_issue_from_reservation(p_center_id bigint, p_reservation_id bigint, p_issue_type character varying, p_payment_mode character varying, p_issued_by bigint, p_notes character varying) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_issue_from_reservation(p_center_id bigint, p_reservation_id bigint, p_issue_type character varying DEFAULT 'Patient'::character varying, p_payment_mode character varying DEFAULT NULL::character varying, p_issued_by bigint DEFAULT NULL::bigint, p_notes character varying DEFAULT NULL::character varying)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
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
        WHERE e.BloodRequestId = p_reservation_id AND e.OverallResult = 'Pass') THEN
        RAISE EXCEPTION 'Reservation has not passed cross-match';
    END IF;

    -- Process each reserved component
    FOR v_detail IN SELECT rd.BloodRequestDetailId, rd.ComponentId, rd.ComponentCode,
        rd.BloodGroup, rd.ComponentType, rd.VolumeMl, rd.UnitRate,
        b.BagId
        FROM BloodRequestDetail rd
        JOIN BloodBagMaster b ON b.DonorId IN (SELECT DonorId FROM ComponentMaster WHERE ComponentId = rd.ComponentId)
        WHERE rd.BloodRequestId = p_reservation_id AND rd.Status = 'Reserved'
    LOOP
        -- Update component status
        UPDATE ComponentMaster SET currentstatus = 'Issued'
        WHERE componentid = v_detail.ComponentId AND currentstatus = 'Reserved';

        -- Update detail status
        UPDATE BloodRequestDetail SET Status = 'Issued'
        WHERE BloodRequestDetailId = v_detail.BloodRequestDetailId;

        -- Create issue record
        INSERT INTO IssueRecord (CenterId, ComponentId, BagId, PatientName,
            IssueDate, IssuedByUserId, IssueType, Notes, RelatedBillingId)
        VALUES (p_center_id, v_detail.ComponentId, v_detail.BagId,
            (SELECT PatientName FROM BloodRequest WHERE BloodRequestId = p_reservation_id),
            NOW(), p_issued_by, p_issue_type, p_notes, NULL);

        v_issue_count := v_issue_count + 1;
        v_total_amount := v_total_amount + COALESCE(v_detail.UnitRate, 0);
    END LOOP;

    -- Update reservation status
    UPDATE BloodRequest SET Status = 'Issued'
    WHERE BloodRequestId = p_reservation_id;

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
        FROM BloodRequestDetail rd WHERE rd.BloodRequestId = p_reservation_id AND rd.Status = 'Issued';

        -- Link invoice to reservation
        UPDATE BloodRequest SET InvoiceId = v_invoice_id
        WHERE BloodRequestId = p_reservation_id;
    END IF;

    RETURN v_issue_count;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_issue_get_by_reservation(p_reservation_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_issue_get_by_reservation(p_reservation_id bigint)
 RETURNS TABLE(issuerecordid bigint, componentid bigint, componentcode character varying, componenttype character varying, bloodgroup character varying, patientname character varying, issuedate timestamp with time zone, issuetype character varying, notes character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT i.IssueRecordId, i.ComponentId,
        c.componentcode, c.componenttype, d.bloodgroup,
        i.PatientName, i.IssueDate, i.IssueType, i.Notes
    FROM IssueRecord i
    JOIN ComponentMaster c ON c.componentid = i.ComponentId
    JOIN BloodBagMaster b ON b.bagid = i.BagId
    JOIN DonorMaster d ON d.donorid = b.DonorId
    WHERE i.RelatedBillingId IN (
        SELECT InvoiceId FROM BloodRequest WHERE BloodRequestId = p_reservation_id
    )
    ORDER BY i.IssueDate;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_issue_get_ready_for_issue(p_center_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_issue_get_ready_for_issue(p_center_id bigint)
 RETURNS TABLE(BloodRequestId bigint, patientname character varying, requiredbloodgroup character varying, componenttype character varying, unitsreserved integer, hospitalname character varying, crossmatchentryid bigint, overallresult character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT r.BloodRequestId, r.PatientName, r.RequiredBloodGroup,
        r.ComponentType, r.UnitsReserved, r.HospitalName,
        e.CrossMatchEntryId, e.OverallResult
    FROM BloodRequest r
    JOIN CrossMatchEntry e ON e.BloodRequestId = r.BloodRequestId
    WHERE r.CenterId = p_center_id AND r.Status = 'Active'
        AND e.OverallResult = 'Pass'
    ORDER BY r.CreatedAt;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_patient_request_create(p_center_id bigint, p_hospital_id bigint, p_patient_name character varying, p_age integer, p_gender character varying, p_blood_group character varying, p_component_type character varying, p_units integer, p_urgency character varying, p_requested_by bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_patient_request_create(p_center_id bigint, p_hospital_id bigint, p_patient_name character varying, p_age integer, p_gender character varying, p_blood_group character varying, p_component_type character varying, p_units integer, p_urgency character varying, p_requested_by bigint)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BloodRequest (CenterId, RequestType, HospitalId, PatientName, PatientAge,
        PatientGender, PatientBloodGroup, RequiredBloodGroup, ComponentType, UnitsRequested,
        RequestUrgency, RequestedByUserId, Status, ReservationDate, CreatedAt)
    VALUES (p_center_id, 'Patient', p_hospital_id, p_patient_name, p_age, p_gender,
        p_blood_group, p_blood_group, p_component_type, p_units,
        p_urgency, p_requested_by, 'Pending', CURRENT_DATE, NOW())
    RETURNING BloodRequestId INTO v_id;
    RETURN v_id;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_patient_request_get_all(p_center_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_patient_request_get_all(p_center_id bigint)
 RETURNS TABLE(requestid bigint, patientname character varying, bloodgroup character varying, componenttype character varying, unitsrequested integer, requesturgency character varying, requestdate timestamp with time zone, hospitalname character varying, relatedissueid bigint)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.BloodRequestId AS RequestId, br.PatientName, br.PatientBloodGroup AS BloodGroup,
            br.ComponentType, br.UnitsRequested, br.RequestUrgency,
            br.CreatedAt AS RequestDate,
            COALESCE(h.HospitalName, '')::VARCHAR AS HospitalName,
            br.RelatedIssueId
        FROM BloodRequest br
        LEFT JOIN HospitalMaster h ON h.HospitalId = br.HospitalId
        WHERE br.CenterId = p_center_id AND br.RequestType = 'Patient'
        ORDER BY br.CreatedAt DESC;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_patient_request_get_by_id(p_center_id bigint, p_request_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_patient_request_get_by_id(p_center_id bigint, p_request_id bigint)
 RETURNS TABLE(requestid bigint, centerid bigint, hospitalid bigint, patientname character varying, patientage integer, patientgender character varying, bloodgroup character varying, componenttype character varying, unitsrequested integer, requestdate timestamp with time zone, requesturgency character varying, requestedbyuserid bigint, relatedissueid bigint, hospitalname character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.BloodRequestId AS RequestId, br.CenterId, br.HospitalId,
            br.PatientName, br.PatientAge, br.PatientGender, br.PatientBloodGroup AS BloodGroup,
            br.ComponentType, br.UnitsRequested, br.CreatedAt AS RequestDate, br.RequestUrgency,
            br.RequestedByUserId, br.RelatedIssueId,
            COALESCE(h.HospitalName, '')::VARCHAR
        FROM BloodRequest br
        LEFT JOIN HospitalMaster h ON h.HospitalId = br.HospitalId
        WHERE br.BloodRequestId = p_request_id AND br.CenterId = p_center_id AND br.RequestType = 'Patient';
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_patient_request_get_pending(p_center_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_patient_request_get_pending(p_center_id bigint)
 RETURNS TABLE(requestid bigint, patientname character varying, bloodgroup character varying, componenttype character varying, unitsrequested integer, requesturgency character varying, requestdate timestamp with time zone, hospitalname character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.BloodRequestId AS RequestId, br.PatientName, br.PatientBloodGroup AS BloodGroup,
            br.ComponentType, br.UnitsRequested, br.RequestUrgency,
            br.CreatedAt AS RequestDate,
            COALESCE(h.HospitalName, '')::VARCHAR AS HospitalName
        FROM BloodRequest br
        LEFT JOIN HospitalMaster h ON h.HospitalId = br.HospitalId
        WHERE br.CenterId = p_center_id AND br.RequestType = 'Patient'
            AND br.RelatedIssueId IS NULL
        ORDER BY br.RequestUrgency = 'Emergency' DESC, br.CreatedAt;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_patient_request_link_issue(p_center_id bigint, p_request_id bigint, p_issue_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_patient_request_link_issue(p_center_id bigint, p_request_id bigint, p_issue_id bigint)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE BloodRequest SET RelatedIssueId = p_issue_id, Status = 'Issued'
    WHERE BloodRequestId = p_patient_id AND CenterId = p_center_id AND RequestType = 'Patient';
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_replacement_donor_get_all(p_center_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_replacement_donor_get_all(p_center_id bigint)
 RETURNS TABLE(replacementdonorid bigint, patientrequestid bigint, donorid bigint, donorname character varying, patientname character varying, donatedat timestamp with time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT rd.ReplacementDonorId, rd.PatientRequestId, rd.DonorId,
            COALESCE(d.FirstName || ' ' || COALESCE(d.LastName, ''), '')::VARCHAR AS DonorName,
            COALESCE(br.PatientName, '')::VARCHAR AS PatientName,
            rd.DonatedAt
        FROM ReplacementDonor rd
        LEFT JOIN DonorMaster d ON d.DonorId = rd.DonorId
        LEFT JOIN BloodRequest br ON br.BloodRequestId = rd.PatientRequestId
        WHERE rd.CenterId = p_center_id
        ORDER BY rd.DonatedAt DESC;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_report_crossmatch(p_center_id bigint, p_invoice_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_report_crossmatch(p_center_id bigint, p_invoice_id bigint)
 RETURNS TABLE(invoice_id bigint, patient_name character varying, patient_address character varying, patient_blood_group character varying, hospital_name character varying, ward character varying, reservation_id bigint, component_code character varying, component_type character varying, blood_group character varying, overall_result character varying, test_type character varying, test_result character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        bt.BillingTransactionId AS invoice_id,
        COALESCE(pr.PatientName, 'N/A')::VARCHAR,
        COALESCE(pr.PatientAddress, '')::VARCHAR,
        COALESCE(pr.PatientBloodGroup, '')::VARCHAR,
        COALESCE(pr.HospitalName, '')::VARCHAR,
        COALESCE(pr.Ward, '')::VARCHAR,
        pr.BloodRequestId,
        COALESCE(cm.ComponentCode, '')::VARCHAR,
        COALESCE(cm.ComponentType, '')::VARCHAR,
        COALESCE(d.BloodGroup, '')::VARCHAR,
        ce.OverallResult::VARCHAR,
        ctr.TestType::VARCHAR,
        ctr.Result::VARCHAR
    FROM BillingTransaction bt
    JOIN BloodRequest pr ON pr.InvoiceId = bt.BillingTransactionId
    JOIN CrossMatchEntry ce ON ce.BloodRequestId = pr.BloodRequestId
    JOIN CrossMatchTestResult ctr ON ctr.CrossMatchEntryId = ce.CrossMatchEntryId
    LEFT JOIN BloodRequestDetail rd ON rd.BloodRequestDetailId = ctr.BloodRequestDetailId
    LEFT JOIN ComponentMaster cm ON cm.ComponentId = rd.ComponentId
    LEFT JOIN BloodBagMaster bbm ON bbm.BagId = cm.BagId
    LEFT JOIN DonorMaster d ON d.DonorId = bbm.DonorId
    WHERE bt.BillingTransactionId = p_invoice_id
        AND bt.CenterId = p_center_id
    ORDER BY ctr.TestResultId;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_reservation_cancel(p_reservation_id bigint, p_reason character varying) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_reservation_cancel(p_reservation_id bigint, p_reason character varying DEFAULT NULL::character varying)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
DECLARE v_comp RECORD;
BEGIN
    -- Release all reserved components back to Available
    FOR v_comp IN SELECT ComponentId FROM BloodRequestDetail
        WHERE BloodRequestId = p_reservation_id AND Status = 'Reserved'
    LOOP
        UPDATE ComponentMaster SET currentstatus = 'Available'
        WHERE componentid = v_comp.ComponentId AND currentstatus = 'Reserved';
    END LOOP;

    -- Update detail statuses
    UPDATE BloodRequestDetail SET Status = 'Released'
    WHERE BloodRequestId = p_reservation_id AND Status = 'Reserved';

    -- Update reservation
    UPDATE BloodRequest SET Status = 'Cancelled', Notes = COALESCE(Notes || ' | ', '') || COALESCE(p_reason, 'Cancelled')
    WHERE BloodRequestId = p_reservation_id;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_reservation_create(p_center_id bigint, p_patient_name character varying, p_patient_address character varying, p_patient_contact_no character varying, p_patient_blood_group character varying, p_required_blood_group character varying, p_hospital_name character varying, p_ward character varying, p_component_type character varying, p_units integer, p_create_invoice boolean, p_created_by bigint, p_notes character varying) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_reservation_create(p_center_id bigint, p_patient_name character varying, p_patient_address character varying, p_patient_contact_no character varying, p_patient_blood_group character varying, p_required_blood_group character varying, p_hospital_name character varying, p_ward character varying, p_component_type character varying, p_units integer, p_create_invoice boolean DEFAULT false, p_created_by bigint DEFAULT NULL::bigint, p_notes character varying DEFAULT NULL::character varying)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_reservation_id BIGINT;
    v_invoice_id BIGINT;
    v_comp RECORD;
    v_units_reserved INT := 0;
    v_total_amount NUMERIC := 0;
    v_inv_no VARCHAR;
BEGIN
    -- Create reservation header
    INSERT INTO BloodRequest (CenterId, PatientName, PatientAddress, PatientContactNo,
        PatientBloodGroup, RequiredBloodGroup, HospitalName, Ward, ComponentType,
        UnitsRequested, ReservationDate, Status, Notes, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_patient_name, p_patient_address, p_patient_contact_no,
        p_patient_blood_group, p_required_blood_group, p_hospital_name, p_ward, p_component_type,
        p_units, CURRENT_DATE, 'Active', p_notes, NOW(), p_created_by)
    RETURNING BloodRequestId INTO v_reservation_id;

    -- FIFO allocate components
    FOR v_comp IN SELECT * FROM fn_reservation_get_available_components(
        p_center_id, p_required_blood_group, p_component_type, p_units)
    LOOP
        -- Lock the component (set status to Reserved)
        UPDATE ComponentMaster SET currentstatus = 'Reserved'
        WHERE componentid = v_comp.ComponentId AND currentstatus = 'Available';

        IF FOUND THEN
            -- Create reservation detail
            INSERT INTO BloodRequestDetail (BloodRequestId, ComponentId, ComponentCode,
                BloodGroup, ComponentType, VolumeMl, ExpiryDate, UnitRate, ReservationRate, Status)
            VALUES (v_reservation_id, v_comp.ComponentId, v_comp.ComponentCode,
                v_comp.BloodGroup, v_comp.ComponentType, v_comp.VolumeMl, v_comp.ExpiryDate,
                v_comp.Rate, v_comp.ReservationRate, 'Reserved');

            v_units_reserved := v_units_reserved + 1;
            v_total_amount := v_total_amount + v_comp.Rate;
        END IF;

        EXIT WHEN v_units_reserved >= p_units;
    END LOOP;

    -- Update reservation with actual count
    UPDATE BloodRequest SET UnitsReserved = v_units_reserved
    WHERE BloodRequestId = v_reservation_id;

    -- Create invoice if requested
    IF p_create_invoice AND v_units_reserved > 0 THEN
        v_inv_no := 'INV-R-' || v_reservation_id || '-' || TO_CHAR(NOW(), 'YYYYMMDD');

        INSERT INTO BillingTransaction (CenterId, InvoiceNumber, PatientId, TotalAmount,
            PaymentStatus, PaymentMode, InvoiceDate, CreatedAt, CreatedBy)
        VALUES (p_center_id, v_inv_no, NULL, v_total_amount,
            'Pending', 'Credit', NOW(), NOW(), p_created_by)
        RETURNING BillingTransactionId INTO v_invoice_id;

        INSERT INTO InvoiceDetail (BillingTransactionId, ComponentId, ServiceName, Quantity, UnitPrice, LineTotal)
        SELECT v_invoice_id, rd.ComponentId, 'Blood - ' || rd.ComponentType || ' (' || rd.BloodGroup || ')',
            1, rd.UnitRate, rd.UnitRate
        FROM BloodRequestDetail rd WHERE rd.BloodRequestId = v_reservation_id;

        UPDATE BloodRequest SET InvoiceId = v_invoice_id
        WHERE BloodRequestId = v_reservation_id;
    END IF;

    RETURN v_reservation_id;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_reservation_get_by_center(p_center_id bigint, p_status character varying, p_from_date date, p_to_date date, p_keyword character varying) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_reservation_get_by_center(p_center_id bigint, p_status character varying DEFAULT NULL::character varying, p_from_date date DEFAULT NULL::date, p_to_date date DEFAULT NULL::date, p_keyword character varying DEFAULT NULL::character varying)
 RETURNS TABLE(BloodRequestId bigint, patientname character varying, patientbloodgroup character varying, requiredbloodgroup character varying, hospitalname character varying, componenttype character varying, unitsrequested integer, unitsreserved integer, status character varying, reservationdate date, invoiceid bigint, notes character varying, createdat timestamp with time zone, createdby bigint)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT r.BloodRequestId, r.PatientName, r.PatientBloodGroup,
        r.RequiredBloodGroup, r.HospitalName, r.ComponentType,
        r.UnitsRequested, r.UnitsReserved, r.Status,
        r.ReservationDate, r.InvoiceId, r.Notes,
        r.CreatedAt, r.CreatedBy
    FROM BloodRequest r
    WHERE r.CenterId = p_center_id
        AND (p_status IS NULL OR r.Status = p_status)
        AND (p_from_date IS NULL OR r.ReservationDate >= p_from_date)
        AND (p_to_date IS NULL OR r.ReservationDate <= p_to_date)
        AND (p_keyword IS NULL OR
            r.PatientName ILIKE '%' || p_keyword || '%' OR
            r.HospitalName ILIKE '%' || p_keyword || '%')
    ORDER BY r.CreatedAt DESC;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_reservation_get_by_id(p_reservation_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_reservation_get_by_id(p_reservation_id bigint)
 RETURNS TABLE(BloodRequestId bigint, patientname character varying, patientaddress character varying, patientcontactno character varying, patientbloodgroup character varying, requiredbloodgroup character varying, hospitalname character varying, ward character varying, componenttype character varying, unitsrequested integer, unitsreserved integer, status character varying, reservationdate date, invoiceid bigint, notes character varying, createdat timestamp with time zone, createdby bigint)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT r.BloodRequestId, r.PatientName, r.PatientAddress,
        r.PatientContactNo, r.PatientBloodGroup, r.RequiredBloodGroup,
        r.HospitalName, r.Ward, r.ComponentType,
        r.UnitsRequested, r.UnitsReserved, r.Status,
        r.ReservationDate, r.InvoiceId, r.Notes,
        r.CreatedAt, r.CreatedBy
    FROM BloodRequest r WHERE r.BloodRequestId = p_reservation_id;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_reservation_get_details(p_reservation_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_reservation_get_details(p_reservation_id bigint)
 RETURNS TABLE(BloodRequestDetailId bigint, componentid bigint, componentcode character varying, bloodgroup character varying, componenttype character varying, volumeml integer, expirydate date, unitrate numeric, reservationrate numeric, status character varying, createdat timestamp with time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT rd.BloodRequestDetailId, rd.ComponentId, rd.ComponentCode,
        rd.BloodGroup, rd.ComponentType, rd.VolumeMl, rd.ExpiryDate,
        rd.UnitRate, rd.ReservationRate, rd.Status, rd.CreatedAt
    FROM BloodRequestDetail rd
    WHERE rd.BloodRequestId = p_reservation_id
    ORDER BY rd.BloodRequestDetailId;
END;
$function$;
DROP FUNCTION IF EXISTS public.fn_reservation_get_pending(p_center_id bigint) CASCADE;
CREATE OR REPLACE FUNCTION public.fn_reservation_get_pending(p_center_id bigint)
 RETURNS TABLE(BloodRequestId bigint, patientname character varying, patientbloodgroup character varying, requiredbloodgroup character varying, hospitalname character varying, componenttype character varying, unitsreserved integer, reservationdate date)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY SELECT r.BloodRequestId, r.PatientName, r.PatientBloodGroup,
        r.RequiredBloodGroup, r.HospitalName, r.ComponentType,
        r.UnitsReserved, r.ReservationDate
    FROM BloodRequest r
    WHERE r.CenterId = p_center_id AND r.Status = 'Active'
    ORDER BY r.CreatedAt;
END;
$function$;
COMMIT;
