-- Fix SPs: use INTERVAL for TimeSpan params, fix ambiguous column in donor_get_by_id

DROP FUNCTION IF EXISTS fn_camp_create(BIGINT, VARCHAR, VARCHAR, BIGINT, VARCHAR, VARCHAR, TIMESTAMP, TIMESTAMP, TIMESTAMP, INTEGER, BIGINT);

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
        p_date::DATE, p_start::TIME, p_end::TIME, p_expected, NOW(), p_created_by)
    RETURNING CampId INTO v_id;
    RETURN v_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_donor_get_by_id(p_donor_id BIGINT)
 RETURNS TABLE(donorid BIGINT, centerid BIGINT, donorcode VARCHAR, firstname VARCHAR,
    lastname VARCHAR, gender VARCHAR, dateofbirth DATE, bloodgroup VARCHAR,
    phone VARCHAR, email VARCHAR, aadhaarnumber VARCHAR, addressline1 VARCHAR,
    addressline2 VARCHAR, city VARCHAR, pincode VARCHAR, occupation VARCHAR,
    preferredlanguage VARCHAR, lastdonationdate DATE, totaldonations INTEGER,
    createdat TIMESTAMPTZ, createdby BIGINT)
 LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY SELECT d.donorid, d.centerid, d.donorcode, d.firstname, d.lastname,
        d.gender, d.dateofbirth, d.bloodgroup, d.phone, d.email, d.aadhaarnumber,
        d.addressline1, d.addressline2, d.city, d.pincode, d.occupation,
        d.preferredlanguage, d.lastdonationdate, d.totaldonations, d.createdat,
        d.createdby
    FROM donormaster d WHERE d.donorid = p_donor_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_donor_update(
    p_donor_id BIGINT, p_center_id BIGINT, p_code VARCHAR, p_first_name VARCHAR,
    p_last_name VARCHAR, p_gender VARCHAR, p_dob TIMESTAMP, p_blood_group VARCHAR,
    p_phone VARCHAR, p_email VARCHAR, p_aadhaar VARCHAR, p_addr1 VARCHAR,
    p_addr2 VARCHAR, p_city VARCHAR, p_pincode VARCHAR, p_occupation VARCHAR,
    p_language VARCHAR, p_updated_by BIGINT
) RETURNS VOID LANGUAGE plpgsql AS $$
BEGIN
    UPDATE DonorMaster SET
        CenterId = COALESCE(p_center_id, CenterId),
        DonorCode = COALESCE(p_code, DonorCode),
        FirstName = COALESCE(p_first_name, FirstName),
        LastName = COALESCE(p_last_name, LastName),
        Gender = COALESCE(p_gender, Gender),
        DateOfBirth = COALESCE(p_dob::DATE, DateOfBirth),
        BloodGroup = COALESCE(p_blood_group, BloodGroup),
        Phone = COALESCE(p_phone, Phone),
        Email = COALESCE(p_email, Email),
        AadhaarNumber = COALESCE(p_aadhaar, AadhaarNumber),
        AddressLine1 = COALESCE(p_addr1, AddressLine1),
        AddressLine2 = COALESCE(p_addr2, AddressLine2),
        City = COALESCE(p_city, City),
        Pincode = COALESCE(p_pincode, Pincode),
        Occupation = COALESCE(p_occupation, Occupation),
        PreferredLanguage = COALESCE(p_language, PreferredLanguage),
        UpdatedAt = NOW(),
        UpdatedBy = p_updated_by
    WHERE DonorId = p_donor_id;
END;
$$;

CREATE OR REPLACE FUNCTION fn_donor_search(
    p_center_id BIGINT, p_keyword VARCHAR, p_blood_group VARCHAR,
    p_gender VARCHAR, p_page INTEGER, p_size INTEGER
) RETURNS TABLE(donorid BIGINT, centerid BIGINT, donorcode VARCHAR, firstname VARCHAR,
    lastname VARCHAR, gender VARCHAR, dateofbirth DATE, bloodgroup VARCHAR,
    phone VARCHAR, email VARCHAR, aadhaarnumber VARCHAR, addressline1 VARCHAR,
    addressline2 VARCHAR, city VARCHAR, pincode VARCHAR, occupation VARCHAR,
    preferredlanguage VARCHAR, lastdonationdate DATE, totaldonations INTEGER,
    createdat TIMESTAMPTZ, createdby BIGINT, totalcount BIGINT)
LANGUAGE plpgsql AS $$
DECLARE v_total BIGINT;
BEGIN
    SELECT COUNT(*) INTO v_total FROM DonorMaster d
        WHERE (p_center_id IS NULL OR d.CenterId = p_center_id)
        AND (p_keyword IS NULL OR d.FirstName ILIKE '%' || p_keyword || '%' OR d.LastName ILIKE '%' || p_keyword || '%' OR d.Phone ILIKE '%' || p_keyword || '%')
        AND (p_blood_group IS NULL OR d.BloodGroup = p_blood_group)
        AND (p_gender IS NULL OR d.Gender = p_gender);

    RETURN QUERY SELECT d.donorid, d.centerid, d.donorcode, d.firstname, d.lastname,
        d.gender, d.dateofbirth, d.bloodgroup, d.phone, d.email, d.aadhaarnumber,
        d.addressline1, d.addressline2, d.city, d.pincode, d.occupation,
        d.preferredlanguage, d.lastdonationdate, d.totaldonations, d.createdat,
        d.createdby, v_total AS totalcount
    FROM donormaster d
        WHERE (p_center_id IS NULL OR d.CenterId = p_center_id)
        AND (p_keyword IS NULL OR d.FirstName ILIKE '%' || p_keyword || '%' OR d.LastName ILIKE '%' || p_keyword || '%' OR d.Phone ILIKE '%' || p_keyword || '%')
        AND (p_blood_group IS NULL OR d.BloodGroup = p_blood_group)
        AND (p_gender IS NULL OR d.Gender = p_gender)
    ORDER BY d.donorid
    LIMIT p_size OFFSET (p_page - 1) * p_size;
END;
$$;

DROP FUNCTION IF EXISTS fn_donor_get_by_phone(BIGINT, VARCHAR);

CREATE OR REPLACE FUNCTION fn_donor_get_by_phone(
    p_center_id BIGINT, p_phone VARCHAR
) RETURNS TABLE(donorid BIGINT, centerid BIGINT, donorcode VARCHAR, firstname VARCHAR,
    lastname VARCHAR, gender VARCHAR, dateofbirth DATE, bloodgroup VARCHAR,
    phone VARCHAR, email VARCHAR, aadhaarnumber VARCHAR, addressline1 VARCHAR,
    addressline2 VARCHAR, city VARCHAR, pincode VARCHAR, occupation VARCHAR,
    preferredlanguage VARCHAR, lastdonationdate DATE, totaldonations INTEGER,
    createdat TIMESTAMPTZ, createdby BIGINT)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY SELECT d.donorid, d.centerid, d.donorcode, d.firstname, d.lastname,
        d.gender, d.dateofbirth, d.bloodgroup, d.phone, d.email, d.aadhaarnumber,
        d.addressline1, d.addressline2, d.city, d.pincode, d.occupation,
        d.preferredlanguage, d.lastdonationdate, d.totaldonations, d.createdat,
        d.createdby
    FROM donormaster d WHERE d.centerid = p_center_id AND d.phone = p_phone;
END;
$$;
