-- ============================================================================
-- BloodCenterOS — v2.0 Schema Migration, Step 1C: FULL ID RENAME
-- Rename reservation* identity columns to bloodrequest* at the physical layer,
-- then re-publish every stored function using the new names.
--
-- Column mapping:
--   bloodrequest.reservationid            -> bloodrequestid
--   bloodrequestdetail.reservationid      -> bloodrequestid
--   bloodrequestdetail.reservationdetailid-> bloodrequestdetailid
--   crossmatchentry.reservationid         -> bloodrequestid
--   crossmatchtestresult.reservationdetailid -> bloodrequestdetailid
--   patientreservation_reservationid_seq  -> bloodrequest_bloodrequestid_seq
--   reservationdetail_reservationdetailid_seq -> bloodrequestdetail_bloodrequestdetailid_seq
-- FK constraints follow the PK renames automatically (Postgres rewrites them).
--
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260810_018d_full_id_rename.sql
-- ============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- 1. Rename identity columns
-- ---------------------------------------------------------------------------
ALTER TABLE BloodRequest      RENAME COLUMN reservationid      TO bloodrequestid;
ALTER TABLE BloodRequestDetail RENAME COLUMN reservationid      TO bloodrequestid;
ALTER TABLE BloodRequestDetail RENAME COLUMN reservationdetailid TO bloodrequestdetailid;
ALTER TABLE CrossMatchEntry    RENAME COLUMN reservationid      TO bloodrequestid;
ALTER TABLE CrossMatchTestResult RENAME COLUMN reservationdetailid TO bloodrequestdetailid;

-- ---------------------------------------------------------------------------
-- 2. Rename sequences and re-point column defaults
-- ---------------------------------------------------------------------------
ALTER SEQUENCE IF EXISTS patientreservation_reservationid_seq   RENAME TO bloodrequest_bloodrequestid_seq;
ALTER SEQUENCE IF EXISTS reservationdetail_reservationdetailid_seq RENAME TO bloodrequestdetail_bloodrequestdetailid_seq;

ALTER TABLE BloodRequest ALTER COLUMN bloodrequestid
    SET DEFAULT nextval('bloodrequest_bloodrequestid_seq'::regclass);
ALTER TABLE BloodRequestDetail ALTER COLUMN bloodrequestdetailid
    SET DEFAULT nextval('bloodrequestdetail_bloodrequestdetailid_seq'::regclass);

COMMIT;