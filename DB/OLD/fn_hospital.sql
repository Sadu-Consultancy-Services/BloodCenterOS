-- ============================================================================
-- Stored Procedures: HospitalMaster
-- ============================================================================

DROP FUNCTION IF EXISTS fn_hospital_get_by_center(BIGINT);
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
