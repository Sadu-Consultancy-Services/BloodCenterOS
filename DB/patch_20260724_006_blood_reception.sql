-- ============================================================================
-- BloodCenterOS — Patch 20260724-006: Blood Reception from MBB + Procurement Register
-- Description: Blood bag reception from Mother Blood Banks with auto-component
--   generation, plus the unified Procurement Register search/summary SPs.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_006_blood_reception.sql
-- ============================================================================

-- 1. Tables -------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS BloodReception (
    ReceptionId     BIGSERIAL PRIMARY KEY,
    CenterId        BIGINT NOT NULL DEFAULT 0,
    MBBName         VARCHAR(300) NOT NULL,
    ReceiptDate     DATE NOT NULL DEFAULT CURRENT_DATE,
    BillNumber      VARCHAR(100),
    TotalBags       INT NOT NULL DEFAULT 0,
    Notes           VARCHAR(2000),
    ReceivedBy      BIGINT,
    CreatedAt       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS BloodReceptionDetail (
    ReceptionDetailId BIGSERIAL PRIMARY KEY,
    ReceptionId     BIGINT NOT NULL REFERENCES BloodReception(ReceptionId) ON DELETE CASCADE,
    DonorName       VARCHAR(200) NOT NULL,
    Sex             VARCHAR(20),
    BloodGroup      VARCHAR(10) NOT NULL,
    ContactNo       VARCHAR(50),
    BagNumber       VARCHAR(100) NOT NULL,
    BagType         VARCHAR(10) NOT NULL,   -- SB, DB, TB, QB
    ExpiryDate      DATE,
    VolumeMl        INT DEFAULT 350,
    CreatedAt       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Blood Reception SPs ------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_blood_reception_create(
    p_center_id BIGINT,
    p_mbb_name VARCHAR,
    p_receipt_date DATE,
    p_bill_number VARCHAR,
    p_notes VARCHAR,
    p_received_by BIGINT,
    p_details JSONB
) RETURNS BIGINT AS $$
DECLARE
    v_reception_id BIGINT;
    v_bag_count INT := 0;
    v_donor_id BIGINT;
    v_bag_id BIGINT;
    v_comp_id BIGINT;
    v_detail JSONB;
    v_donor_name VARCHAR;
    v_sex VARCHAR;
    v_blood_group VARCHAR;
    v_contact_no VARCHAR;
    v_bag_number VARCHAR;
    v_bag_type VARCHAR;
    v_expiry_date DATE;
    v_volume_ml INT;
BEGIN
    INSERT INTO BloodReception (CenterId, MBBName, ReceiptDate, BillNumber, Notes, ReceivedBy)
    VALUES (p_center_id, p_mbb_name, p_receipt_date, p_bill_number, p_notes, p_received_by)
    RETURNING ReceptionId INTO v_reception_id;

    FOR v_detail IN SELECT * FROM jsonb_array_elements(p_details)
    LOOP
        v_donor_name := v_detail->>'donorName';
        v_sex := v_detail->>'sex';
        v_blood_group := v_detail->>'bloodGroup';
        v_contact_no := v_detail->>'contactNo';
        v_bag_number := v_detail->>'bagNumber';
        v_bag_type := v_detail->>'bagType';
        v_expiry_date := (v_detail->>'expiryDate')::DATE;
        v_volume_ml := COALESCE((v_detail->>'volumeMl')::INT, 350);

        IF EXISTS (SELECT 1 FROM BloodBagMaster WHERE bloodbagnumber = v_bag_number) THEN
            CONTINUE;
        END IF;

        INSERT INTO DonorMaster (centerid, firstname, gender, bloodgroup, phone)
        VALUES (p_center_id, v_donor_name, v_sex, v_blood_group, v_contact_no)
        RETURNING donorid INTO v_donor_id;

        INSERT INTO BloodBagMaster (centerid, bloodbagnumber, donorid, bagtype, bagstatus,
            bagvolumeml, expirydate, initialcollectedat)
        VALUES (p_center_id, v_bag_number, v_donor_id, v_bag_type, 'Available',
            v_volume_ml, v_expiry_date, p_receipt_date::TIMESTAMPTZ)
        RETURNING bagid INTO v_bag_id;

        INSERT INTO DonorDonationHistory (centerid, donorid, donationtype, volumeml, bagnumber)
        VALUES (p_center_id, v_donor_id, 'MBB', v_volume_ml, v_bag_number);

        IF v_bag_type = 'SB' THEN
            INSERT INTO ComponentPreparation (centerid, parentbagid, componenttype, volumeml, preparedat)
            VALUES (p_center_id, v_bag_id, 'WB', v_volume_ml, NOW());

            INSERT INTO ComponentMaster (centerid, componentcode, parentbagid, componenttype, volumeml, currentstatus, expirydate)
            VALUES (p_center_id, 'CMP-MBB-' || v_bag_id || '-WB', v_bag_id, 'WB', v_volume_ml, 'Available', v_expiry_date);

        ELSIF v_bag_type = 'DB' THEN
            INSERT INTO ComponentPreparation (centerid, parentbagid, componenttype, volumeml, preparedat)
            VALUES (p_center_id, v_bag_id, 'PCV', v_volume_ml * 2 / 3, NOW());

            INSERT INTO ComponentMaster (centerid, componentcode, parentbagid, componenttype, volumeml, currentstatus, expirydate)
            VALUES (p_center_id, 'CMP-MBB-' || v_bag_id || '-PCV', v_bag_id, 'PCV', v_volume_ml * 2 / 3, 'Available', v_expiry_date);

            INSERT INTO ComponentPreparation (centerid, parentbagid, componenttype, volumeml, preparedat)
            VALUES (p_center_id, v_bag_id, 'FFP', v_volume_ml / 3, NOW());

            INSERT INTO ComponentMaster (centerid, componentcode, parentbagid, componenttype, volumeml, currentstatus, expirydate)
            VALUES (p_center_id, 'CMP-MBB-' || v_bag_id || '-FFP', v_bag_id, 'FFP', v_volume_ml / 3, 'Available', v_expiry_date);

        ELSIF v_bag_type IN ('TB', 'QB') THEN
            INSERT INTO ComponentPreparation (centerid, parentbagid, componenttype, volumeml, preparedat)
            VALUES (p_center_id, v_bag_id, 'PCV', v_volume_ml / 2, NOW());

            INSERT INTO ComponentMaster (centerid, componentcode, parentbagid, componenttype, volumeml, currentstatus, expirydate)
            VALUES (p_center_id, 'CMP-MBB-' || v_bag_id || '-PCV', v_bag_id, 'PCV', v_volume_ml / 2, 'Available', v_expiry_date);

            INSERT INTO ComponentPreparation (centerid, parentbagid, componenttype, volumeml, preparedat)
            VALUES (p_center_id, v_bag_id, 'FFP', v_volume_ml / 3, NOW());

            INSERT INTO ComponentMaster (centerid, componentcode, parentbagid, componenttype, volumeml, currentstatus, expirydate)
            VALUES (p_center_id, 'CMP-MBB-' || v_bag_id || '-FFP', v_bag_id, 'FFP', v_volume_ml / 3, 'Available', v_expiry_date);

            INSERT INTO ComponentPreparation (centerid, parentbagid, componenttype, volumeml, preparedat)
            VALUES (p_center_id, v_bag_id, 'PC', v_volume_ml / 6, NOW());

            INSERT INTO ComponentMaster (centerid, componentcode, parentbagid, componenttype, volumeml, currentstatus, expirydate)
            VALUES (p_center_id, 'CMP-MBB-' || v_bag_id || '-PC', v_bag_id, 'PC', v_volume_ml / 6, 'Available', v_expiry_date);
        END IF;

        INSERT INTO InventoryStock (centerid, componenttype, bloodgroup, availableqty, lastupdatedat)
        VALUES (p_center_id, 'WB', v_blood_group,
            CASE WHEN v_bag_type = 'SB' THEN 1 ELSE 0 END, NOW())
        ON CONFLICT (centerid, COALESCE(componenttype, ''), COALESCE(bloodgroup, ''))
        DO UPDATE SET availableqty = InventoryStock.availableqty +
            CASE WHEN v_bag_type = 'SB' THEN 1 ELSE 0 END,
            lastupdatedat = NOW();

        INSERT INTO InventoryStock (centerid, componenttype, bloodgroup, availableqty, lastupdatedat)
        VALUES (p_center_id, 'PCV', v_blood_group,
            CASE WHEN v_bag_type IN ('DB', 'TB', 'QB') THEN 1 ELSE 0 END, NOW())
        ON CONFLICT (centerid, COALESCE(componenttype, ''), COALESCE(bloodgroup, ''))
        DO UPDATE SET availableqty = InventoryStock.availableqty +
            CASE WHEN v_bag_type IN ('DB', 'TB', 'QB') THEN 1 ELSE 0 END,
            lastupdatedat = NOW();

        INSERT INTO InventoryStock (centerid, componenttype, bloodgroup, availableqty, lastupdatedat)
        VALUES (p_center_id, 'FFP', v_blood_group,
            CASE WHEN v_bag_type IN ('DB', 'TB', 'QB') THEN 1 ELSE 0 END, NOW())
        ON CONFLICT (centerid, COALESCE(componenttype, ''), COALESCE(bloodgroup, ''))
        DO UPDATE SET availableqty = InventoryStock.availableqty +
            CASE WHEN v_bag_type IN ('DB', 'TB', 'QB') THEN 1 ELSE 0 END,
            lastupdatedat = NOW();

        INSERT INTO InventoryStock (centerid, componenttype, bloodgroup, availableqty, lastupdatedat)
        VALUES (p_center_id, 'PC', v_blood_group,
            CASE WHEN v_bag_type IN ('TB', 'QB') THEN 1 ELSE 0 END, NOW())
        ON CONFLICT (centerid, COALESCE(componenttype, ''), COALESCE(bloodgroup, ''))
        DO UPDATE SET availableqty = InventoryStock.availableqty +
            CASE WHEN v_bag_type IN ('TB', 'QB') THEN 1 ELSE 0 END,
            lastupdatedat = NOW();

        INSERT INTO BloodReceptionDetail (ReceptionId, DonorName, Sex, BloodGroup, ContactNo,
            BagNumber, BagType, ExpiryDate, VolumeMl)
        VALUES (v_reception_id, v_donor_name, v_sex, v_blood_group, v_contact_no,
            v_bag_number, v_bag_type, v_expiry_date, v_volume_ml);

        v_bag_count := v_bag_count + 1;
    END LOOP;

    UPDATE BloodReception SET TotalBags = v_bag_count WHERE ReceptionId = v_reception_id;

    RETURN v_reception_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_blood_reception_get_by_id(p_reception_id BIGINT)
RETURNS TABLE(
    ReceptionId BIGINT, CenterId BIGINT, MBBName VARCHAR, ReceiptDate DATE,
    BillNumber VARCHAR, TotalBags INT, Notes VARCHAR, ReceivedBy BIGINT, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT r.ReceptionId, r.CenterId, r.MBBName, r.ReceiptDate,
        r.BillNumber, r.TotalBags, r.Notes, r.ReceivedBy, r.CreatedAt
    FROM BloodReception r WHERE r.ReceptionId = p_reception_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_blood_reception_get_details(p_reception_id BIGINT)
RETURNS TABLE(
    ReceptionDetailId BIGINT, ReceptionId BIGINT, DonorName VARCHAR, Sex VARCHAR,
    BloodGroup VARCHAR, ContactNo VARCHAR, BagNumber VARCHAR, BagType VARCHAR,
    ExpiryDate DATE, VolumeMl INT, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT d.ReceptionDetailId, d.ReceptionId, d.DonorName, d.Sex,
        d.BloodGroup, d.ContactNo, d.BagNumber, d.BagType,
        d.ExpiryDate, d.VolumeMl, d.CreatedAt
    FROM BloodReceptionDetail d
    WHERE d.ReceptionId = p_reception_id
    ORDER BY d.ReceptionDetailId;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_blood_reception_get_by_center(
    p_center_id BIGINT,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL
)
RETURNS TABLE(
    ReceptionId BIGINT, CenterId BIGINT, MBBName VARCHAR, ReceiptDate DATE,
    BillNumber VARCHAR, TotalBags INT, Notes VARCHAR, ReceivedBy BIGINT, CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT r.ReceptionId, r.CenterId, r.MBBName, r.ReceiptDate,
        r.BillNumber, r.TotalBags, r.Notes, r.ReceivedBy, r.CreatedAt
    FROM BloodReception r
    WHERE r.CenterId = p_center_id
        AND (p_from_date IS NULL OR r.ReceiptDate >= p_from_date)
        AND (p_to_date IS NULL OR r.ReceiptDate <= p_to_date)
    ORDER BY r.ReceiptDate DESC, r.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;


-- 3. Procurement Register SPs -------------------------------------------------

CREATE OR REPLACE FUNCTION fn_procurement_register_search(
    p_center_id BIGINT,
    p_blood_group VARCHAR DEFAULT NULL,
    p_component_type VARCHAR DEFAULT NULL,
    p_status VARCHAR DEFAULT NULL,
    p_from_date DATE DEFAULT NULL,
    p_to_date DATE DEFAULT NULL,
    p_keyword VARCHAR DEFAULT NULL
)
RETURNS TABLE(
    RegisterId BIGINT,
    ComponentId BIGINT,
    ComponentCode VARCHAR,
    ComponentType VARCHAR,
    VolumeMl INT,
    BloodGroup VARCHAR,
    BagNumber VARCHAR,
    BagType VARCHAR,
    DonorName VARCHAR,
    DonorId BIGINT,
    Status VARCHAR,
    ExpiryDate DATE,
    StorageLocation VARCHAR,
    CreatedAt TIMESTAMPTZ
) AS $$
BEGIN
    RETURN QUERY SELECT
        c.componentid AS RegisterId,
        c.componentid,
        c.componentcode,
        c.componenttype,
        c.volumeml::INT,
        d.bloodgroup,
        b.bloodbagnumber AS BagNumber,
        b.bagtype,
        (d.firstname || ' ' || COALESCE(d.lastname, ''))::VARCHAR AS DonorName,
        d.donorid,
        c.currentstatus AS Status,
        c.expirydate,
        c.storagelocation,
        c.createdat
    FROM ComponentMaster c
    LEFT JOIN BloodBagMaster b ON b.bagid = c.parentbagid
    LEFT JOIN DonorMaster d ON d.donorid = b.donorid
    WHERE c.centerid = p_center_id
        AND (p_blood_group IS NULL OR d.bloodgroup = p_blood_group)
        AND (p_component_type IS NULL OR c.componenttype = p_component_type)
        AND (p_status IS NULL OR c.currentstatus = p_status)
        AND (p_from_date IS NULL OR c.createdat::DATE >= p_from_date)
        AND (p_to_date IS NULL OR c.createdat::DATE <= p_to_date)
        AND (p_keyword IS NULL OR
            d.firstname ILIKE '%' || p_keyword || '%' OR
            d.lastname ILIKE '%' || p_keyword || '%' OR
            b.bloodbagnumber ILIKE '%' || p_keyword || '%' OR
            c.componentcode ILIKE '%' || p_keyword || '%')
    ORDER BY c.expirydate, c.createdat DESC;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION fn_procurement_register_summary(
    p_center_id BIGINT
)
RETURNS TABLE(
    BloodGroup VARCHAR,
    ComponentType VARCHAR,
    Available INT,
    Reserved INT,
    Issued INT,
    Discarded INT,
    Total INT
) AS $$
BEGIN
    RETURN QUERY
    WITH bg AS (
        SELECT DISTINCT d.bloodgroup FROM DonorMaster d WHERE d.centerid = p_center_id AND d.bloodgroup IS NOT NULL
        UNION SELECT 'A+' UNION SELECT 'A-' UNION SELECT 'B+' UNION SELECT 'B-'
        UNION SELECT 'AB+' UNION SELECT 'AB-' UNION SELECT 'O+' UNION SELECT 'O-'
    ),
    ct AS (
        SELECT DISTINCT c.componenttype FROM ComponentMaster c WHERE c.centerid = p_center_id AND c.componenttype IS NOT NULL
        UNION SELECT 'WB' UNION SELECT 'PCV' UNION SELECT 'FFP' UNION SELECT 'PC'
    )
    SELECT
        bg.bloodgroup::VARCHAR,
        ct.componenttype::VARCHAR,
        COALESCE(SUM(CASE WHEN c.currentstatus = 'Available' THEN 1 ELSE 0 END), 0)::INT AS Available,
        COALESCE(SUM(CASE WHEN c.currentstatus = 'Reserved' THEN 1 ELSE 0 END), 0)::INT AS Reserved,
        COALESCE(SUM(CASE WHEN c.currentstatus = 'Issued' THEN 1 ELSE 0 END), 0)::INT AS Issued,
        COALESCE(SUM(CASE WHEN c.currentstatus = 'Discarded' THEN 1 ELSE 0 END), 0)::INT AS Discarded,
        COUNT(c.componentid)::INT AS Total
    FROM bg
    CROSS JOIN ct
    LEFT JOIN ComponentMaster c ON c.centerid = p_center_id
        AND c.componenttype = ct.componenttype
        AND EXISTS (SELECT 1 FROM BloodBagMaster b2 JOIN DonorMaster d2 ON d2.donorid = b2.donorid
            WHERE b2.bagid = c.parentbagid AND d2.bloodgroup = bg.bloodgroup)
    GROUP BY bg.bloodgroup, ct.componenttype
    HAVING COUNT(c.componentid) > 0
    ORDER BY bg.bloodgroup, ct.componenttype;
END;
$$ LANGUAGE plpgsql;
