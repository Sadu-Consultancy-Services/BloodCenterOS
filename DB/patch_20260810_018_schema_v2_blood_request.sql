-- ============================================================================
-- BloodCenterOS — v2.0 Schema Migration, Step 1B (PART A: structure + data)
-- Unify PatientRequest + PatientReservation + EmergencyRequest into BloodRequest.
--
-- Naming decision: the physical tables are renamed BloodRequest /
-- BloodRequestDetail (the canonical entity), while the original column NAMES
-- (reservationid, reservationdetailid, patientname, ...) are retained so that
-- all existing stored-function output aliases and C# Dapper column mappings
-- keep working. New union columns are added for the Patient/Emergency flows.
--
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260810_018_schema_v2_blood_request.sql
-- ============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- 1. Rename canonical request tables (FK constraints follow automatically)
-- ---------------------------------------------------------------------------
ALTER TABLE IF EXISTS PatientReservation RENAME TO BloodRequest;
ALTER TABLE IF EXISTS ReservationDetail   RENAME TO BloodRequestDetail;

-- ---------------------------------------------------------------------------
-- 2. Extend BloodRequest with union columns for Patient/Emergency flows
--    (column names kept snake_case, matching Dapper mappings for new model)
-- ---------------------------------------------------------------------------
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS RequestType            VARCHAR(20) NOT NULL DEFAULT 'Reservation';
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS hospitalid             BIGINT;
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS patientgender          VARCHAR(50);
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS requesturgency         VARCHAR(50);
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS prescriptionattachmentid BIGINT;
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS requestedbyuserid      BIGINT;
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS relatedissueid         BIGINT;
ALTER TABLE BloodRequest ADD COLUMN IF NOT EXISTS fulfilledat            TIMESTAMPTZ;

-- ---------------------------------------------------------------------------
-- 3. Migrate PatientRequest rows (RequestType = 'Patient')
-- ---------------------------------------------------------------------------
INSERT INTO BloodRequest
    (centerid, requesttype, patientname, patientbloodgroup, requiredbloodgroup,
     hospitalname, hospitalid, patientgender, componenttype, unitsrequested,
     requesturgency, prescriptionattachmentid, requestedbyuserid, relatedissueid,
     status, createdat)
SELECT
     pr.centerid, 'Patient', pr.patientname, pr.bloodgroup, pr.bloodgroup,
     h.hospitalname, pr.hospitalid, pr.patientage, pr.componenttype, pr.unitsrequested,
     pr.requesturgency, pr.prescriptionattachmentid, pr.requestedbyuserid, pr.relatedissueid,
     CASE WHEN pr.relatedissueid IS NOT NULL THEN 'Issued' ELSE 'Pending' END,
     pr.requestdate
FROM PatientRequest pr
LEFT JOIN HospitalMaster h ON h.HospitalId = pr.HospitalId;

-- ---------------------------------------------------------------------------
-- 4. Migrate EmergencyRequest rows (RequestType = 'Emergency')
-- ---------------------------------------------------------------------------
INSERT INTO BloodRequest
    (centerid, requesttype, patientname, patientbloodgroup, requiredbloodgroup,
     hospitalname, hospitalid, componenttype, unitsrequested, requestedbyuserid,
     requesturgency, fulfilledat, status, createdat)
SELECT
    e.centerid, 'Emergency', e.patientname, e.bloodgroup, e.bloodgroup,
     h.hospitalname, e.hospitalid, e.componenttype, e.unitsrequired, e.requestedbyuserid,
     'Emergency', e.fulfilledat, COALESCE(e.requeststatus, 'Pending'), e.requestedat
FROM EmergencyRequest e
LEFT JOIN HospitalMaster h ON h.HospitalId = e.HospitalId;

-- ---------------------------------------------------------------------------
-- 5. Archive legacy tables
-- ---------------------------------------------------------------------------
ALTER TABLE IF EXISTS PatientRequest   RENAME TO PatientRequest_legacy;
ALTER TABLE IF EXISTS EmergencyRequest RENAME TO EmergencyRequest_legacy;

COMMIT;