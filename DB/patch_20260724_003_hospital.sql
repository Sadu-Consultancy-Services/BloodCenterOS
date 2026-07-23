-- ============================================================================
-- BloodCenterOS — Patch 20260724-003: Hospital Master full CRUD
-- Description: Seed data + update/delete SPs for HospitalMaster table
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_003_hospital.sql
-- ============================================================================

-- 1. Seed Data ---------------------------------------------------------------

INSERT INTO HospitalMaster (CenterId, HospitalCode, HospitalName, Address, ContactPerson, Phone, Email, CreatedAt)
SELECT 1, NULL, name, address, contact, phone, email, NOW()
FROM (VALUES
    ('City General Hospital',      '123 MG Road, Mumbai',       'Dr. Sharma', '022-24567890', 'contact@citygen.in'),
    ('Apex Medical Center',        '45 Park Avenue, Delhi',     'Dr. Verma',  '011-23456789', 'info@apexmed.in'),
    ('Sunrise Hospital & Research','88 Lake View Road, Bangalore','Dr. Nair',  '080-34567890', 'admin@sunrise.in'),
    ('Lifeline Super Speciality',  '12 Civil Lines, Pune',      'Dr. Joshi',  '020-45678901', 'contact@lifeline.in')
) AS t(name, address, contact, phone, email)
WHERE NOT EXISTS (SELECT 1 FROM HospitalMaster WHERE HospitalName = t.name);

-- 2. Stored Procedures -------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_hospital_get_by_id(p_hospital_id BIGINT)
RETURNS TABLE(hospitalid BIGINT, centerid BIGINT, hospitalcode VARCHAR, hospitalname VARCHAR,
    address VARCHAR, contactperson VARCHAR, phone VARCHAR, email VARCHAR, createdat TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT h.HospitalId, h.CenterId, h.HospitalCode, h.HospitalName, h.Address,
        h.ContactPerson, h.Phone, h.Email, h.CreatedAt
    FROM HospitalMaster h
    WHERE h.HospitalId = p_hospital_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_hospital_get_by_center(p_center_id BIGINT)
RETURNS TABLE(hospitalid BIGINT, centerid BIGINT, hospitalcode VARCHAR, hospitalname VARCHAR,
    address VARCHAR, contactperson VARCHAR, phone VARCHAR, email VARCHAR, createdat TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT h.HospitalId, h.CenterId, h.HospitalCode, h.HospitalName, h.Address,
        h.ContactPerson, h.Phone, h.Email, h.CreatedAt
    FROM HospitalMaster h
    WHERE h.CenterId = p_center_id
    ORDER BY h.HospitalName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_hospital_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_address VARCHAR,
    p_contact VARCHAR, p_phone VARCHAR, p_email VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO HospitalMaster (CenterId, HospitalCode, HospitalName, Address,
        ContactPerson, Phone, Email, CreatedAt)
    VALUES (p_center_id, p_code, p_name, p_address, p_contact, p_phone, p_email, NOW())
    RETURNING HospitalId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_hospital_update(
    p_hospital_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_address VARCHAR,
    p_contact VARCHAR, p_phone VARCHAR, p_email VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE HospitalMaster SET
        HospitalCode = COALESCE(p_code, HospitalCode),
        HospitalName = COALESCE(p_name, HospitalName),
        Address = COALESCE(p_address, Address),
        ContactPerson = COALESCE(p_contact, ContactPerson),
        Phone = COALESCE(p_phone, Phone),
        Email = COALESCE(p_email, Email)
    WHERE HospitalId = p_hospital_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_hospital_delete(p_hospital_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM HospitalMaster WHERE HospitalId = p_hospital_id;
END;
$$ LANGUAGE plpgsql;
