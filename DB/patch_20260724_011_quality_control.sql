-- ============================================================================
-- BloodCenterOS — Patch 20260724-011: Quality Control
-- Description: 6-type QC tabbed interface (Pool Cell, Anticera, Saline,
--   Copper Sulphate, Coombs/AHG, BSA) with QCRegister table.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_011_quality_control.sql
-- ============================================================================

-- 1. QCRegister Table --------------------------------------------------------

CREATE TABLE IF NOT EXISTS QCRegister (
    QCRecordId      BIGSERIAL PRIMARY KEY,
    CenterId        BIGINT NOT NULL DEFAULT 0,
    QCType          VARCHAR(50) NOT NULL,       -- PoolCell, Anticera, Saline, CopperSulphate, CoombsAHG, BSA
    QCDate          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PerformedBy     BIGINT,

    -- Pool Cell Register
    UnitNumber      VARCHAR(100),
    Specificity     VARCHAR(100),
    BatchNo         VARCHAR(100),

    -- Anticera / BSA / CoombsAHG
    Expiry          DATE,
    Reactivity      VARCHAR(100),
    Activity        VARCHAR(100),
    Titre           VARCHAR(100),

    -- Normal Saline
    Appearance      VARCHAR(200),
    Haemolysis      VARCHAR(100),
    SpGravity       VARCHAR(100),

    -- Copper Sulphate
    HighControl     VARCHAR(100),
    LowControl      VARCHAR(100),

    DeviceId        BIGINT,
    Notes           VARCHAR(2000),
    CreatedAt       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. QC SPs ------------------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_qc_create(
    p_center_id BIGINT,
    p_qc_type VARCHAR,
    p_qc_date TIMESTAMPTZ,
    p_performed_by BIGINT,
    p_device_id BIGINT DEFAULT NULL,
    p_unit_number VARCHAR DEFAULT NULL,
    p_specificity VARCHAR DEFAULT NULL,
    p_batch_no VARCHAR DEFAULT NULL,
    p_expiry DATE DEFAULT NULL,
    p_reactivity VARCHAR DEFAULT NULL,
    p_activity VARCHAR DEFAULT NULL,
    p_titre VARCHAR DEFAULT NULL,
    p_appearance VARCHAR DEFAULT NULL,
    p_haemolysis VARCHAR DEFAULT NULL,
    p_sp_gravity VARCHAR DEFAULT NULL,
    p_high_control VARCHAR DEFAULT NULL,
    p_low_control VARCHAR DEFAULT NULL,
    p_notes VARCHAR DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO QCRegister (CenterId, QCType, QCDate, PerformedBy, DeviceId,
        UnitNumber, Specificity, BatchNo, Expiry, Reactivity, Activity, Titre,
        Appearance, Haemolysis, SpGravity, HighControl, LowControl, Notes)
    VALUES (p_center_id, p_qc_type, p_qc_date, p_performed_by, p_device_id,
        p_unit_number, p_specificity, p_batch_no, p_expiry, p_reactivity, p_activity, p_titre,
        p_appearance, p_haemolysis, p_sp_gravity, p_high_control, p_low_control, p_notes)
    RETURNING QCRecordId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_qc_get_by_center(
    p_center_id BIGINT,
    p_qc_type VARCHAR DEFAULT NULL,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL
)
RETURNS TABLE(
    QCRecordId BIGINT, QCType VARCHAR, QCDate TIMESTAMPTZ, PerformedBy BIGINT,
    DeviceId BIGINT, UnitNumber VARCHAR, Specificity VARCHAR, BatchNo VARCHAR,
    Expiry DATE, Reactivity VARCHAR, Activity VARCHAR, Titre VARCHAR,
    Appearance VARCHAR, Haemolysis VARCHAR, SpGravity VARCHAR,
    HighControl VARCHAR, LowControl VARCHAR, Notes VARCHAR, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT q.QCRecordId, q.QCType, q.QCDate, q.PerformedBy,
        q.DeviceId, q.UnitNumber, q.Specificity, q.BatchNo,
        q.Expiry, q.Reactivity, q.Activity, q.Titre,
        q.Appearance, q.Haemolysis, q.SpGravity,
        q.HighControl, q.LowControl, q.Notes, q.CreatedAt
    FROM QCRegister q
    WHERE q.CenterId = p_center_id
        AND (p_qc_type IS NULL OR q.QCType = p_qc_type)
        AND (p_from_date IS NULL OR q.QCDate::DATE >= p_from_date)
        AND (p_to_date IS NULL OR q.QCDate::DATE <= p_to_date)
    ORDER BY q.QCDate DESC;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_qc_get_by_id(p_qc_id BIGINT)
RETURNS TABLE(
    QCRecordId BIGINT, QCType VARCHAR, QCDate TIMESTAMPTZ, PerformedBy BIGINT,
    DeviceId BIGINT, UnitNumber VARCHAR, Specificity VARCHAR, BatchNo VARCHAR,
    Expiry DATE, Reactivity VARCHAR, Activity VARCHAR, Titre VARCHAR,
    Appearance VARCHAR, Haemolysis VARCHAR, SpGravity VARCHAR,
    HighControl VARCHAR, LowControl VARCHAR, Notes VARCHAR, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT q.QCRecordId, q.QCType, q.QCDate, q.PerformedBy,
        q.DeviceId, q.UnitNumber, q.Specificity, q.BatchNo,
        q.Expiry, q.Reactivity, q.Activity, q.Titre,
        q.Appearance, q.Haemolysis, q.SpGravity,
        q.HighControl, q.LowControl, q.Notes, q.CreatedAt
    FROM QCRegister q WHERE q.QCRecordId = p_qc_id;
END;
$$ LANGUAGE plpgsql;
