-- ============================================================================
-- BloodCenterOS — Patch 20260724-007: Rate Management + Patient Reservation
-- Description: Blood component rate management per blood group + FIFO-based
--   patient reservation with optional invoicing.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_007_rates_reservation.sql
-- ============================================================================

-- 1. RateMaster Table --------------------------------------------------------

CREATE TABLE IF NOT EXISTS RateMaster (
    RateId          BIGSERIAL PRIMARY KEY,
    CenterId        BIGINT NOT NULL DEFAULT 0,
    BloodGroup      VARCHAR(10) NOT NULL,
    ComponentType   VARCHAR(50) NOT NULL,
    UnitRate        NUMERIC(18,2) NOT NULL DEFAULT 0,
    ReservationRate NUMERIC(18,2) NOT NULL DEFAULT 0,
    IsActive        BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UpdatedAt       TIMESTAMPTZ,
    UpdatedBy       BIGINT,
    UNIQUE (CenterId, BloodGroup, ComponentType)
);

-- 2. Rate Management SPs -----------------------------------------------------

CREATE OR REPLACE FUNCTION fn_rate_upsert(
    p_center_id BIGINT,
    p_blood_group VARCHAR,
    p_component_type VARCHAR,
    p_unit_rate NUMERIC,
    p_reservation_rate NUMERIC,
    p_updated_by BIGINT DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO RateMaster (CenterId, BloodGroup, ComponentType, UnitRate, ReservationRate, UpdatedAt, UpdatedBy)
    VALUES (p_center_id, p_blood_group, p_component_type, p_unit_rate, p_reservation_rate, NOW(), p_updated_by)
    ON CONFLICT (CenterId, BloodGroup, ComponentType)
    DO UPDATE SET UnitRate = p_unit_rate, ReservationRate = p_reservation_rate,
        IsActive = TRUE, UpdatedAt = NOW(), UpdatedBy = p_updated_by
    RETURNING RateId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_rate_get_by_center(p_center_id BIGINT)
RETURNS TABLE(
    RateId BIGINT, CenterId BIGINT, BloodGroup VARCHAR, ComponentType VARCHAR,
    UnitRate NUMERIC, ReservationRate NUMERIC, IsActive BOOLEAN, CreatedAt TIMESTAMPTZ, UpdatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT r.RateId, r.CenterId, r.BloodGroup, r.ComponentType,
        r.UnitRate, r.ReservationRate, r.IsActive, r.CreatedAt, r.UpdatedAt
    FROM RateMaster r
    WHERE r.CenterId = p_center_id
    ORDER BY r.BloodGroup, r.ComponentType;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_rate_get_by_id(p_rate_id BIGINT)
RETURNS TABLE(
    RateId BIGINT, CenterId BIGINT, BloodGroup VARCHAR, ComponentType VARCHAR,
    UnitRate NUMERIC, ReservationRate NUMERIC, IsActive BOOLEAN, CreatedAt TIMESTAMPTZ, UpdatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT r.RateId, r.CenterId, r.BloodGroup, r.ComponentType,
        r.UnitRate, r.ReservationRate, r.IsActive, r.CreatedAt, r.UpdatedAt
    FROM RateMaster r WHERE r.RateId = p_rate_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_rate_delete(p_rate_id BIGINT) RETURNS VOID AS $$
BEGIN
    UPDATE RateMaster SET IsActive = FALSE WHERE RateId = p_rate_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_rate_get_default(
    p_center_id BIGINT,
    p_blood_group VARCHAR,
    p_component_type VARCHAR
) RETURNS TABLE(
    RateId BIGINT, BloodGroup VARCHAR, ComponentType VARCHAR, UnitRate NUMERIC, ReservationRate NUMERIC
) AS $$
BEGIN
    RETURN QUERY SELECT r.RateId, r.BloodGroup, r.ComponentType, r.UnitRate, r.ReservationRate
    FROM RateMaster r
    WHERE r.CenterId = p_center_id
        AND r.BloodGroup = p_blood_group
        AND r.ComponentType = p_component_type
        AND r.IsActive = TRUE
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;


-- 3. PatientReservation Tables -----------------------------------------------

CREATE TABLE IF NOT EXISTS PatientReservation (
    ReservationId   BIGSERIAL PRIMARY KEY,
    CenterId        BIGINT NOT NULL DEFAULT 0,
    PatientName     VARCHAR(300) NOT NULL,
    PatientAddress  VARCHAR(500),
    PatientContactNo VARCHAR(50),
    PatientBloodGroup VARCHAR(10) NOT NULL,
    RequiredBloodGroup VARCHAR(10) NOT NULL,
    HospitalName    VARCHAR(300),
    Ward            VARCHAR(200),
    ComponentType   VARCHAR(50) NOT NULL,
    UnitsRequested  INT NOT NULL DEFAULT 1,
    UnitsReserved   INT NOT NULL DEFAULT 0,
    ReservationDate DATE NOT NULL DEFAULT CURRENT_DATE,
    Status          VARCHAR(50) NOT NULL DEFAULT 'Active',  -- Active, Cancelled, Issued
    InvoiceId       BIGINT,
    Notes           VARCHAR(2000),
    CreatedAt       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CreatedBy       BIGINT,
    UpdatedAt       TIMESTAMPTZ,
    UpdatedBy       BIGINT
);

CREATE TABLE IF NOT EXISTS ReservationDetail (
    ReservationDetailId BIGSERIAL PRIMARY KEY,
    ReservationId   BIGINT NOT NULL REFERENCES PatientReservation(ReservationId) ON DELETE CASCADE,
    ComponentId     BIGINT NOT NULL,
    ComponentCode   VARCHAR(100),
    BloodGroup      VARCHAR(10),
    ComponentType   VARCHAR(50),
    VolumeMl        INT,
    ExpiryDate      DATE,
    UnitRate        NUMERIC(18,2) DEFAULT 0,
    ReservationRate NUMERIC(18,2) DEFAULT 0,
    Status          VARCHAR(50) NOT NULL DEFAULT 'Reserved',  -- Reserved, Issued, Released
    CreatedAt       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 4. Reservation SPs ---------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_reservation_get_available_components(
    p_center_id BIGINT,
    p_blood_group VARCHAR,
    p_component_type VARCHAR,
    p_units INT
)
RETURNS TABLE(
    ComponentId BIGINT, ComponentCode VARCHAR, ComponentType VARCHAR,
    VolumeMl INT, BloodGroup VARCHAR, ExpiryDate DATE, StorageLocation VARCHAR,
    Rate NUMERIC, ReservationRate NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.componentid, c.componentcode, c.componenttype,
        c.volumeml::INT, d.bloodgroup, c.expirydate, c.storagelocation,
        COALESCE(r.unitrate, 0), COALESCE(r.reservationrate, 0)
    FROM ComponentMaster c
    JOIN BloodBagMaster b ON b.bagid = c.parentbagid
    JOIN DonorMaster d ON d.donorid = b.donorid
    LEFT JOIN RateMaster r ON r.centerid = p_center_id
        AND r.bloodgroup = d.bloodgroup
        AND r.componenttype = c.componenttype
        AND r.isactive = TRUE
    WHERE c.centerid = p_center_id
        AND c.currentstatus = 'Available'
        AND d.bloodgroup = p_blood_group
        AND c.componenttype = p_component_type
        AND (c.expirydate IS NULL OR c.expirydate >= CURRENT_DATE)
    ORDER BY c.expirydate, c.componentid   -- FIFO: oldest expiry first
    LIMIT p_units;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_reservation_create(
    p_center_id BIGINT,
    p_patient_name VARCHAR,
    p_patient_address VARCHAR,
    p_patient_contact_no VARCHAR,
    p_patient_blood_group VARCHAR,
    p_required_blood_group VARCHAR,
    p_hospital_name VARCHAR,
    p_ward VARCHAR,
    p_component_type VARCHAR,
    p_units INT,
    p_create_invoice BOOLEAN DEFAULT FALSE,
    p_created_by BIGINT DEFAULT NULL,
    p_notes VARCHAR DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE
    v_reservation_id BIGINT;
    v_invoice_id BIGINT;
    v_comp RECORD;
    v_units_reserved INT := 0;
    v_total_amount NUMERIC := 0;
    v_inv_no VARCHAR;
BEGIN
    -- Create reservation header
    INSERT INTO PatientReservation (CenterId, PatientName, PatientAddress, PatientContactNo,
        PatientBloodGroup, RequiredBloodGroup, HospitalName, Ward, ComponentType,
        UnitsRequested, ReservationDate, Status, Notes, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_patient_name, p_patient_address, p_patient_contact_no,
        p_patient_blood_group, p_required_blood_group, p_hospital_name, p_ward, p_component_type,
        p_units, CURRENT_DATE, 'Active', p_notes, NOW(), p_created_by)
    RETURNING ReservationId INTO v_reservation_id;

    -- FIFO allocate components
    FOR v_comp IN SELECT * FROM fn_reservation_get_available_components(
        p_center_id, p_required_blood_group, p_component_type, p_units)
    LOOP
        -- Lock the component (set status to Reserved)
        UPDATE ComponentMaster SET currentstatus = 'Reserved'
        WHERE componentid = v_comp.ComponentId AND currentstatus = 'Available';

        IF FOUND THEN
            -- Create reservation detail
            INSERT INTO ReservationDetail (ReservationId, ComponentId, ComponentCode,
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
    UPDATE PatientReservation SET UnitsReserved = v_units_reserved
    WHERE ReservationId = v_reservation_id;

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
        FROM ReservationDetail rd WHERE rd.ReservationId = v_reservation_id;

        UPDATE PatientReservation SET InvoiceId = v_invoice_id
        WHERE ReservationId = v_reservation_id;
    END IF;

    RETURN v_reservation_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_reservation_get_by_center(
    p_center_id BIGINT,
    p_status VARCHAR DEFAULT NULL,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL,
    p_keyword VARCHAR DEFAULT NULL
)
RETURNS TABLE(
    ReservationId BIGINT, PatientName VARCHAR, PatientBloodGroup VARCHAR,
    RequiredBloodGroup VARCHAR, HospitalName VARCHAR, ComponentType VARCHAR,
    UnitsRequested INT, UnitsReserved INT, Status VARCHAR,
    ReservationDate DATE, InvoiceId BIGINT, Notes VARCHAR,
    CreatedAt TIMESTAMPTZ, CreatedBy BIGINT
) AS $$
BEGIN
    RETURN QUERY SELECT r.ReservationId, r.PatientName, r.PatientBloodGroup,
        r.RequiredBloodGroup, r.HospitalName, r.ComponentType,
        r.UnitsRequested, r.UnitsReserved, r.Status,
        r.ReservationDate, r.InvoiceId, r.Notes,
        r.CreatedAt, r.CreatedBy
    FROM PatientReservation r
    WHERE r.CenterId = p_center_id
        AND (p_status IS NULL OR r.Status = p_status)
        AND (p_from_date IS NULL OR r.ReservationDate >= p_from_date)
        AND (p_to_date IS NULL OR r.ReservationDate <= p_to_date)
        AND (p_keyword IS NULL OR
            r.PatientName ILIKE '%' || p_keyword || '%' OR
            r.HospitalName ILIKE '%' || p_keyword || '%')
    ORDER BY r.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_reservation_get_by_id(p_reservation_id BIGINT)
RETURNS TABLE(
    ReservationId BIGINT, PatientName VARCHAR, PatientAddress VARCHAR,
    PatientContactNo VARCHAR, PatientBloodGroup VARCHAR, RequiredBloodGroup VARCHAR,
    HospitalName VARCHAR, Ward VARCHAR, ComponentType VARCHAR,
    UnitsRequested INT, UnitsReserved INT, Status VARCHAR,
    ReservationDate DATE, InvoiceId BIGINT, Notes VARCHAR,
    CreatedAt TIMESTAMPTZ, CreatedBy BIGINT
) AS $$
BEGIN
    RETURN QUERY SELECT r.ReservationId, r.PatientName, r.PatientAddress,
        r.PatientContactNo, r.PatientBloodGroup, r.RequiredBloodGroup,
        r.HospitalName, r.Ward, r.ComponentType,
        r.UnitsRequested, r.UnitsReserved, r.Status,
        r.ReservationDate, r.InvoiceId, r.Notes,
        r.CreatedAt, r.CreatedBy
    FROM PatientReservation r WHERE r.ReservationId = p_reservation_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_reservation_get_details(p_reservation_id BIGINT)
RETURNS TABLE(
    ReservationDetailId BIGINT, ComponentId BIGINT, ComponentCode VARCHAR,
    BloodGroup VARCHAR, ComponentType VARCHAR, VolumeMl INT, ExpiryDate DATE,
    UnitRate NUMERIC, ReservationRate NUMERIC, Status VARCHAR, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT rd.ReservationDetailId, rd.ComponentId, rd.ComponentCode,
        rd.BloodGroup, rd.ComponentType, rd.VolumeMl, rd.ExpiryDate,
        rd.UnitRate, rd.ReservationRate, rd.Status, rd.CreatedAt
    FROM ReservationDetail rd
    WHERE rd.ReservationId = p_reservation_id
    ORDER BY rd.ReservationDetailId;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_reservation_cancel(
    p_reservation_id BIGINT,
    p_reason VARCHAR DEFAULT NULL
) RETURNS VOID AS $$
DECLARE v_comp RECORD;
BEGIN
    -- Release all reserved components back to Available
    FOR v_comp IN SELECT ComponentId FROM ReservationDetail
        WHERE ReservationId = p_reservation_id AND Status = 'Reserved'
    LOOP
        UPDATE ComponentMaster SET currentstatus = 'Available'
        WHERE componentid = v_comp.ComponentId AND currentstatus = 'Reserved';
    END LOOP;

    -- Update detail statuses
    UPDATE ReservationDetail SET Status = 'Released'
    WHERE ReservationId = p_reservation_id AND Status = 'Reserved';

    -- Update reservation
    UPDATE PatientReservation SET Status = 'Cancelled', Notes = COALESCE(Notes || ' | ', '') || COALESCE(p_reason, 'Cancelled')
    WHERE ReservationId = p_reservation_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_reservation_get_pending(p_center_id BIGINT)
RETURNS TABLE(
    ReservationId BIGINT, PatientName VARCHAR, PatientBloodGroup VARCHAR,
    RequiredBloodGroup VARCHAR, HospitalName VARCHAR, ComponentType VARCHAR,
    UnitsReserved INT, ReservationDate DATE
) AS $$
BEGIN
    RETURN QUERY SELECT r.ReservationId, r.PatientName, r.PatientBloodGroup,
        r.RequiredBloodGroup, r.HospitalName, r.ComponentType,
        r.UnitsReserved, r.ReservationDate
    FROM PatientReservation r
    WHERE r.CenterId = p_center_id AND r.Status = 'Active'
    ORDER BY r.CreatedAt;
END;
$$ LANGUAGE plpgsql;
