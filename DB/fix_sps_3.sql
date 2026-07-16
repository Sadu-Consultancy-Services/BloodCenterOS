-- Fix camp: use current date + interval for timestamp columns
-- Fix collection: accept numeric volume

DROP FUNCTION IF EXISTS fn_camp_create(BIGINT, VARCHAR, VARCHAR, BIGINT, VARCHAR, VARCHAR, TIMESTAMP, INTERVAL, INTERVAL, INTEGER, BIGINT);

CREATE OR REPLACE FUNCTION fn_camp_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_organizer_id BIGINT,
    p_venue VARCHAR, p_city VARCHAR, p_date TIMESTAMP, p_start INTERVAL,
    p_end INTERVAL, p_expected INTEGER, p_created_by BIGINT
) RETURNS BIGINT LANGUAGE plpgsql AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BloodCampMaster (CenterId, CampCode, CampName, OrganizerId, Venue,
        City, CampDate, StartTime, EndTime, TotalDonorsExpected, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_code, p_name, p_organizer_id, p_venue, p_city,
        p_date::DATE, CURRENT_DATE + p_start, CURRENT_DATE + p_end,
        p_expected, NOW(), p_created_by)
    RETURNING CampId INTO v_id;
    RETURN v_id;
END;
$$;

DROP FUNCTION IF EXISTS fn_collection_create(BIGINT, BIGINT, BIGINT, BIGINT, VARCHAR, VARCHAR, VARCHAR, INTEGER, BIGINT, VARCHAR, TIMESTAMPTZ, TIMESTAMPTZ, VARCHAR, BIGINT);

CREATE OR REPLACE FUNCTION fn_collection_create(
    p_center_id BIGINT, p_branch_id BIGINT, p_camp_id BIGINT, p_donor_id BIGINT,
    p_bag_no VARCHAR, p_barcode VARCHAR, p_lot_no VARCHAR, p_volume NUMERIC,
    p_collector_id BIGINT, p_location_type VARCHAR, p_start TIMESTAMPTZ,
    p_end TIMESTAMPTZ, p_notes VARCHAR, p_created_by BIGINT
) RETURNS BIGINT LANGUAGE plpgsql AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CollectionRecord (CenterId, BranchId, CampId, DonorId, BloodBagNumber,
        BagBarcode, BagLotNumber, BagVolumeMl, CollectorEmployeeId, CollectionLocationType,
        CollectionStartTime, CollectionEndTime, Notes, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_branch_id, p_camp_id, p_donor_id, p_bag_no, p_barcode,
        p_lot_no, p_volume, p_collector_id, p_location_type, p_start, p_end,
        p_notes, NOW(), p_created_by)
    RETURNING CollectionId INTO v_id;

    INSERT INTO BloodBagMaster (CenterId, BloodBagNumber, CollectionId, DonorId,
        BagBarcode, BagLotNumber, BagVolumeMl, BagStatus, InitialCollectedAt, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_bag_no, v_id, p_donor_id, p_barcode, p_lot_no,
        p_volume, 'Collected', COALESCE(p_start, NOW()), NOW(), p_created_by);

    PERFORM fn_donor_donation_create(p_center_id, p_donor_id, v_id, 'Voluntary',
        p_volume, p_bag_no, NULL, p_created_by);

    RETURN v_id;
END;
$$;
