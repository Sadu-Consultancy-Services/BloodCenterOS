-- Fix type mismatches for Dapper compatibility

DROP FUNCTION IF EXISTS fn_donor_create(BIGINT, VARCHAR, VARCHAR, VARCHAR, VARCHAR, DATE, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, BIGINT);

CREATE OR REPLACE FUNCTION fn_donor_create(
    p_center_id BIGINT, p_code VARCHAR, p_first_name VARCHAR, p_last_name VARCHAR,
    p_gender VARCHAR, p_dob TIMESTAMP, p_blood_group VARCHAR, p_phone VARCHAR,
    p_email VARCHAR, p_aadhaar VARCHAR, p_addr1 VARCHAR, p_addr2 VARCHAR,
    p_city VARCHAR, p_pincode VARCHAR, p_occupation VARCHAR, p_language VARCHAR,
    p_created_by BIGINT
) RETURNS BIGINT LANGUAGE plpgsql AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DonorMaster (CenterId, DonorCode, FirstName, LastName, Gender,
        DateOfBirth, BloodGroup, Phone, Email, AadhaarNumber, AddressLine1,
        AddressLine2, City, Pincode, Occupation, PreferredLanguage, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_code, p_first_name, p_last_name, p_gender,
        p_dob::DATE, p_blood_group, p_phone, p_email, p_aadhaar, p_addr1,
        p_addr2, p_city, p_pincode, p_occupation, p_language, NOW(), p_created_by)
    RETURNING DonorId INTO v_id;
    RETURN v_id;
END;
$$;

DROP FUNCTION IF EXISTS fn_camp_create(BIGINT, VARCHAR, VARCHAR, BIGINT, VARCHAR, VARCHAR, DATE, TIMESTAMPTZ, TIMESTAMPTZ, INTEGER, BIGINT);

CREATE OR REPLACE FUNCTION fn_camp_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_organizer_id BIGINT,
    p_venue VARCHAR, p_city VARCHAR, p_date TIMESTAMP, p_start TIMESTAMP,
    p_end TIMESTAMP, p_expected INTEGER, p_created_by BIGINT
) RETURNS BIGINT LANGUAGE plpgsql AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BloodCampMaster (CenterId, CampCode, CampName, OrganizerId, Venue,
        City, CampDate, StartTime, EndTime, TotalDonorsExpected, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_code, p_name, p_organizer_id, p_venue, p_city,
        p_date::DATE, p_start::TIME, p_end::TIME, p_expected, NOW(), p_created_by)
    RETURNING CampId INTO v_id;
    RETURN v_id;
END;
$$;

DROP FUNCTION IF EXISTS fn_inventory_upsert(BIGINT, VARCHAR, VARCHAR, INTEGER, INTEGER, INTEGER, BIGINT);

CREATE OR REPLACE FUNCTION fn_inventory_upsert(
    p_center_id BIGINT, p_component_type VARCHAR, p_blood_group VARCHAR,
    p_available INTEGER DEFAULT 0, p_reserved INTEGER DEFAULT 0,
    p_quarantined INTEGER DEFAULT 0, p_updated_by BIGINT DEFAULT NULL
) RETURNS BIGINT LANGUAGE plpgsql AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO InventoryStock (CenterId, ComponentType, BloodGroup, AvailableQty,
        ReservedQty, QuarantinedQty, LastUpdatedAt, LastUpdatedBy, CreatedAt)
    VALUES (p_center_id, p_component_type, p_blood_group, p_available, p_reserved,
        p_quarantined, NOW(), p_updated_by, NOW())
    ON CONFLICT (CenterId, COALESCE(ComponentType, ''), COALESCE(BloodGroup, ''))
    DO UPDATE SET AvailableQty = InventoryStock.AvailableQty + p_available,
        ReservedQty = InventoryStock.ReservedQty + p_reserved,
        QuarantinedQty = InventoryStock.QuarantinedQty + p_quarantined,
        LastUpdatedAt = NOW(), LastUpdatedBy = p_updated_by
    RETURNING InventoryStockId INTO v_id;
    RETURN v_id;
END;
$$;
