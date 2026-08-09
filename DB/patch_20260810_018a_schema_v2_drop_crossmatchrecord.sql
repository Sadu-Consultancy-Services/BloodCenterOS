-- ============================================================================
-- BloodCenterOS — v2.0 Schema Migration, Step 1A
-- Drop CrossMatchRecord (dead legacy table superseded by CrossMatchEntry +
-- CrossMatchTestResult from Patch 008). No live writer exists; verified:
--   - No C# reference other than the orphaned model
--   - Replacement table = crossmatchentry/crossmatchtestresult
--   - Historical rows are preserved in the pre-migration backup
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260810_018a_schema_v2_drop_crossmatchrecord.sql
-- ============================================================================

-- 1. Drop the legacy SP that WROTE into CrossMatchRecord (never called by any repo)
DROP FUNCTION IF EXISTS fn_crossmatch_create(bigint, bigint, bigint, character varying, character varying, bigint);

-- 2. Drop the dead table (orphaned model CrossMatchRecord.cs also deleted in Step 1A)
DROP TABLE IF EXISTS CrossMatchRecord;

-- 3. Verify nothing else depends on it
DO $$
BEGIN
    IF to_regclass('CrossMatchRecord') IS NOT NULL AND EXISTS (
        SELECT 1 FROM pg_depend d
        JOIN pg_rewrite r ON r.oid = d.objid
        JOIN pg_class c ON c.oid = r.ev_class
        WHERE d.refobjid = 'CrossMatchRecord'::regclass
    ) THEN
        RAISE EXCEPTION 'CrossMatchRecord still referenced by a view/rule';
    END IF;
END $$;