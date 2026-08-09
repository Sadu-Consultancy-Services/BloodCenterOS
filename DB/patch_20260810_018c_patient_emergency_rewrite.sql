-- ============================================================================
-- BloodCenterOS — v2.0 Schema Migration, Step 1B (PART B: manual rewrites)
-- Rewrite patient/emergency/replacement functions onto the unified
-- BloodRequest / BloodRequestDetail tables.
--
-- Column-name mapping used (legacy -> unified):
--   patient.requestid       -> bloodrequest.reservationid
--   patient.patientage      -> bloodrequest.patientage        (added below)
--   patient.patientgender   -> bloodrequest.patientgender
--   patient.bloodgroup      -> bloodrequest.patientbloodgroup
--   patient.requestdate     -> bloodrequest.createdat
--   patient.requesturgency  -> bloodrequest.requesturgency
--   patient.prescriptionid  -> bloodrequest.prescriptionattachmentid
--   emergency.requeststatus -> bloodrequest.status
--   emergency.requestedat   -> bloodrequest.createdat
--   emergency.fulfilledat   -> bloodrequest.fulfilledat
--   emergency.unitsrequired -> bloodrequest.unitsrequested
--   replacementdonor.patientrequestid -> bloodrequest.reservationid
--
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260810_018c_patient_emergency_rewrite.sql
-- ============================================================================

BEGIN;

-- patientage is required by the patient-request functions but not yet in
-- the unified table; add it (idempotent).
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS patientage INTEGER;

-- ---------------------------------------------------------------------------
-- Patient flow
-- ---------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.fn_patient_request_create(
    p_center_id bigint,
    p_hospital_id bigint,
    p_patient_name character varying,
    p_age integer,
    p_gender character varying,
    p_blood_group character varying,
    p_component_type character varying,
    p_units integer,
    p_urgency character varying,
    p_requested_by bigint)
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
    RETURNING ReservationId INTO v_id;
    RETURN v_id;
END;
$function$;

CREATE OR REPLACE FUNCTION public.fn_patient_request_get_all(p_center_id bigint)
 RETURNS TABLE(requestid bigint, patientname character varying, bloodgroup character varying,
    componenttype character varying, unitsrequested integer, requesturgency character varying,
    requestdate timestamp with time zone, hospitalname character varying, relatedissueid bigint)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.ReservationId AS RequestId, br.PatientName, br.PatientBloodGroup AS BloodGroup,
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

CREATE OR REPLACE FUNCTION public.fn_patient_request_get_by_id(p_center_id bigint, p_request_id bigint)
 RETURNS TABLE(requestid bigint, centerid bigint, hospitalid bigint, patientname character varying,
    patientage integer, patientgender character varying, bloodgroup character varying,
    componenttype character varying, unitsrequested integer, requestdate timestamp with time zone,
    requesturgency character varying, requestedbyuserid bigint, relatedissueid bigint,
    hospitalname character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.ReservationId AS RequestId, br.CenterId, br.HospitalId,
            br.PatientName, br.PatientAge, br.PatientGender, br.PatientBloodGroup AS BloodGroup,
            br.ComponentType, br.UnitsRequested, br.CreatedAt AS RequestDate, br.RequestUrgency,
            br.RequestedByUserId, br.RelatedIssueId,
            COALESCE(h.HospitalName, '')::VARCHAR
        FROM BloodRequest br
        LEFT JOIN HospitalMaster h ON h.HospitalId = br.HospitalId
        WHERE br.ReservationId = p_request_id AND br.CenterId = p_center_id AND br.RequestType = 'Patient';
END;
$function$;

CREATE OR REPLACE FUNCTION public.fn_patient_request_get_pending(p_center_id bigint)
 RETURNS TABLE(requestid bigint, patientname character varying, bloodgroup character varying,
    componenttype character varying, unitsrequested integer, requesturgency character varying,
    requestdate timestamp with time zone, hospitalname character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.ReservationId AS RequestId, br.PatientName, br.PatientBloodGroup AS BloodGroup,
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

CREATE OR REPLACE FUNCTION public.fn_patient_request_link_issue(
    p_center_id bigint, p_request_id bigint, p_issue_id bigint)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE BloodRequest SET RelatedIssueId = p_issue_id, Status = 'Issued'
    WHERE ReservationId = p_patient_id AND CenterId = p_center_id AND RequestType = 'Patient';
END;
$function$;

-- ---------------------------------------------------------------------------
-- Emergency flow
-- ---------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.fn_emergency_request_create(
    p_center_id bigint,
    p_hospital_id bigint,
    p_patient_name character varying,
    p_blood_group character varying,
    p_component_type character varying,
    p_units integer,
    p_requested_by bigint,
    p_notes character varying)
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
    RETURNING ReservationId INTO v_id;
    RETURN v_id;
END;
$function$;

CREATE OR REPLACE FUNCTION public.fn_emergency_request_get_pending(p_center_id bigint)
 RETURNS TABLE(emergencyrequestid bigint, centerid bigint, hospitalid bigint,
    patientname character varying, bloodgroup character varying, componenttype character varying,
    unitsrequired integer, requeststatus character varying, requestedat timestamp with time zone,
    notes character varying)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
        SELECT br.ReservationId AS EmergencyRequestId, br.CenterId, br.HospitalId,
            br.PatientName, br.PatientBloodGroup AS BloodGroup, br.ComponentType,
            br.UnitsRequested AS UnitsRequired, br.Status AS RequestStatus,
            br.CreatedAt AS RequestedAt, br.Notes
        FROM BloodRequest br
        WHERE br.CenterId = p_center_id AND br.RequestType = 'Emergency'
            AND br.Status IN ('Pending','Processing')
        ORDER BY br.CreatedAt DESC;
END;
$function$;

-- ---------------------------------------------------------------------------
-- Emergency donor response (unchanged: EmergencyDonorResponse table retained,
-- EmergencyRequestId now references BloodRequest.ReservationId)
-- ---------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.fn_emergency_donor_response(
    p_emergency_id bigint, p_donor_id bigint, p_contact character varying)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO EmergencyDonorResponse (EmergencyRequestId, DonorId, ResponseContact,
        RespondedAt, IsVerified)
    VALUES (p_emergency_id, p_donor_id, p_contact, NOW(), FALSE)
    RETURNING ResponseId INTO v_id;
    RETURN v_id;
END;
$function$;

-- ---------------------------------------------------------------------------
-- Replacement donor: link to unified request + build display of the request
-- ---------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.fn_replacement_donor_register(
    p_center_id bigint, p_request_id bigint, p_donor_id bigint)
 RETURNS bigint
 LANGUAGE plpgsql
AS $function$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ReplacementDonor (CenterId, PatientRequestId, DonorId, DonatedAt)
    VALUES (p_center_id, p_request_id, p_donor_id, NOW())
    RETURNING ReplacementDonorId INTO v_id;
    RETURN v_id;
END;
$function$;

CREATE OR REPLACE FUNCTION public.fn_replacement_donor_get_all(p_center_id bigint)
 RETURNS TABLE(replacementdonorid bigint, patientrequestid bigint, donorid bigint,
    donorname character varying, patientname character varying, donatedat timestamp with time zone)
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
        LEFT JOIN BloodRequest br ON br.ReservationId = rd.PatientRequestId
        WHERE rd.CenterId = p_center_id
        ORDER BY rd.DonatedAt DESC;
END;
$function$;

COMMIT;