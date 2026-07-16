CREATE OR REPLACE FUNCTION fn_emergency_request_get_pending(p_center_id BIGINT)
RETURNS TABLE(EmergencyRequestId BIGINT, CenterId BIGINT, HospitalId BIGINT,
    PatientName VARCHAR, BloodGroup VARCHAR, ComponentType VARCHAR,
    UnitsRequired INT, RequestStatus VARCHAR, RequestedAt TIMESTAMPTZ,
    Notes VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT e.EmergencyRequestId, e.CenterId, e.HospitalId,
        e.PatientName, e.BloodGroup, e.ComponentType,
        e.UnitsRequired, e.RequestStatus, e.RequestedAt, e.Notes
    FROM EmergencyRequest e
    WHERE e.CenterId = p_center_id AND e.RequestStatus IN ('Pending','Processing')
    ORDER BY e.RequestedAt DESC;
END;
$$ LANGUAGE plpgsql;
