-- ============================================================================
-- Stored Procedures: BloodCampMaster (list by center)
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_camp_get_by_center(p_center_id BIGINT)
RETURNS TABLE(campid BIGINT, centerid BIGINT, campcode VARCHAR, campname VARCHAR,
    organizerid BIGINT, venue VARCHAR, city VARCHAR, campdate DATE,
    starttime TIMESTAMPTZ, endtime TIMESTAMPTZ, totaldonorsexpected INT,
    totaldonorscollected INT, createdat TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT c.CampId, c.CenterId, c.CampCode, c.CampName,
        c.OrganizerId, c.Venue, c.City, c.CampDate,
        c.StartTime, c.EndTime, c.TotalDonorsExpected,
        c.TotalDonorsCollected, c.CreatedAt
    FROM BloodCampMaster c
    WHERE c.CenterId = p_center_id
    ORDER BY c.CampDate DESC;
END;
$$ LANGUAGE plpgsql;
