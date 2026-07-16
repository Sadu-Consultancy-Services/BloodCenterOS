CREATE OR REPLACE FUNCTION fn_hospital_get_by_center(p_center_id BIGINT)
RETURNS TABLE(HospitalId BIGINT, CenterId BIGINT, HospitalCode VARCHAR, HospitalName VARCHAR,
    Address VARCHAR, ContactPerson VARCHAR, Phone VARCHAR, Email VARCHAR, CreatedAt TIMESTAMP) AS $$
BEGIN
    RETURN QUERY SELECT h.HospitalId, h.CenterId, h.HospitalCode, h.HospitalName, h.Address,
        h.ContactPerson, h.Phone, h.Email, h.CreatedAt
    FROM HospitalMaster h
    WHERE h.CenterId = p_center_id
    ORDER BY h.HospitalName;
END;
$$ LANGUAGE plpgsql;
