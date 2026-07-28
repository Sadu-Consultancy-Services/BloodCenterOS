-- ============================================================================
-- BloodCenterOS — Patch 20260724-010: Blood Discarding + Autoclave Tracking
-- Description: Discard expired/unsuitable blood, Discard Register,
--   auto-reject rule (3+ discards), autoclave sterilization tracking.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_010_discard_autoclave.sql
-- ============================================================================

-- 1. Add autoclave columns to DiscardRecord ---------------------------------

ALTER TABLE DiscardRecord ADD COLUMN IF NOT EXISTS AutoClaveStartTime TIMESTAMPTZ;
ALTER TABLE DiscardRecord ADD COLUMN IF NOT EXISTS AutoClaveEndTime   TIMESTAMPTZ;

-- 2. Discard SPs ------------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_discard_get_available_components(p_center_id BIGINT)
RETURNS TABLE(
    ComponentId BIGINT, ComponentCode VARCHAR, ComponentType VARCHAR,
    BloodGroup VARCHAR, VolumeMl INT, ExpiryDate DATE, BagId BIGINT, BagNo VARCHAR,
    DonorId BIGINT, DonorName VARCHAR
) AS $$
BEGIN
    RETURN QUERY SELECT c.componentid, c.componentcode, c.componenttype,
        b.bloodgroup, c.volumeml, c.expirydate::DATE, bg.bagid, bg.bagno,
        d.donorid, d.firstname || ' ' || d.lastname
    FROM ComponentMaster c
    JOIN BloodBagMaster bg ON bg.bagid = c.bagid
    JOIN DonorMaster d ON d.donorid = bg.donorid
    WHERE c.centerid = p_center_id
        AND c.currentstatus = 'Available'
        AND (d.isrejected IS NULL OR d.isrejected = FALSE)
    ORDER BY c.expirydate;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_discard_bulk(
    p_center_id BIGINT,
    p_component_ids BIGINT[],   -- Array of ComponentIds
    p_reason VARCHAR,
    p_discarded_by BIGINT,
    p_notes VARCHAR DEFAULT NULL
) RETURNS TABLE(DiscardId BIGINT, ComponentId BIGINT) AS $$
DECLARE
    v_cid BIGINT;
    v_bag_id BIGINT;
    v_donor_id BIGINT;
    v_discard_count INT;
    v_discard_id BIGINT;
BEGIN
    FOREACH v_cid IN ARRAY p_component_ids
    LOOP
        -- Get bag and donor
        SELECT c.bagid, b.donorid INTO v_bag_id, v_donor_id
        FROM ComponentMaster c
        JOIN BloodBagMaster b ON b.bagid = c.bagid
        WHERE c.componentid = v_cid AND c.currentstatus = 'Available';

        IF NOT FOUND THEN CONTINUE; END IF;

        -- Insert discard record
        INSERT INTO DiscardRecord (CenterId, BagId, ComponentId, DiscardReason,
            DiscardedAt, DiscardedBy, Notes)
        VALUES (p_center_id, v_bag_id, v_cid, p_reason, NOW(), p_discarded_by, p_notes)
        RETURNING DiscardId INTO v_discard_id;

        -- Update component status
        UPDATE ComponentMaster SET currentstatus = 'Discarded'
        WHERE componentid = v_cid;

        -- Update bag status
        UPDATE BloodBagMaster SET BagStatus = 'Discarded', UpdatedAt = NOW()
        WHERE BagId = v_bag_id;

        -- Auto-reject check: if donor has 3+ discards in this center
        SELECT COUNT(*) INTO v_discard_count FROM DiscardRecord dr
        JOIN ComponentMaster cm ON cm.componentid = dr.componentid
        JOIN BloodBagMaster bm ON bm.bagid = cm.bagid
        WHERE bm.donorid = v_donor_id AND dr.centerid = p_center_id;

        IF v_discard_count >= 3 THEN
            UPDATE DonorMaster SET isrejected = TRUE WHERE donorid = v_donor_id;
        END IF;

        DiscardId := v_discard_id;
        ComponentId := v_cid;
        RETURN NEXT;
    END LOOP;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_discard_get_by_center(
    p_center_id BIGINT,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL,
    p_reason VARCHAR DEFAULT NULL
)
RETURNS TABLE(
    DiscardId BIGINT, ComponentId BIGINT, ComponentCode VARCHAR, ComponentType VARCHAR,
    BloodGroup VARCHAR, BagNo VARCHAR, DiscardReason VARCHAR, DiscardedAt TIMESTAMPTZ,
    DiscardedBy BIGINT, Notes VARCHAR, DonorName VARCHAR,
    AutoClaveStartTime TIMESTAMPTZ, AutoClaveEndTime TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT dr.DiscardId, c.componentid, c.componentcode, c.componenttype,
        b.bloodgroup, bg.bagno, dr.DiscardReason, dr.DiscardedAt, dr.DiscardedBy, dr.Notes,
        d.firstname || ' ' || d.lastname,
        dr.AutoClaveStartTime, dr.AutoClaveEndTime
    FROM DiscardRecord dr
    JOIN ComponentMaster c ON c.componentid = dr.componentid
    JOIN BloodBagMaster bg ON bg.bagid = dr.bagid
    JOIN DonorMaster d ON d.donorid = bg.donorid
    WHERE dr.CenterId = p_center_id
        AND (p_from_date IS NULL OR dr.DiscardedAt::DATE >= p_from_date)
        AND (p_to_date IS NULL OR dr.DiscardedAt::DATE <= p_to_date)
        AND (p_reason IS NULL OR dr.DiscardReason = p_reason)
    ORDER BY dr.DiscardedAt DESC;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_discard_set_autoclave(
    p_discard_id BIGINT,
    p_start_time TIMESTAMPTZ,
    p_end_time TIMESTAMPTZ
) RETURNS VOID AS $$
BEGIN
    UPDATE DiscardRecord
    SET AutoClaveStartTime = p_start_time,
        AutoClaveEndTime = p_end_time
    WHERE DiscardId = p_discard_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_discard_get_autoclave_register(p_center_id BIGINT)
RETURNS TABLE(
    DiscardId BIGINT, ComponentCode VARCHAR, ComponentType VARCHAR,
    BagNo VARCHAR, DiscardedAt TIMESTAMPTZ,
    AutoClaveStartTime TIMESTAMPTZ, AutoClaveEndTime TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT dr.DiscardId, c.componentcode, c.componenttype,
        bg.bagno, dr.DiscardedAt, dr.AutoClaveStartTime, dr.AutoClaveEndTime
    FROM DiscardRecord dr
    JOIN ComponentMaster c ON c.componentid = dr.componentid
    JOIN BloodBagMaster bg ON bg.bagid = dr.bagid
    WHERE dr.CenterId = p_center_id
        AND (dr.AutoClaveStartTime IS NOT NULL OR dr.AutoClaveEndTime IS NOT NULL)
    ORDER BY dr.DiscardedAt DESC;
END;
$$ LANGUAGE plpgsql;
