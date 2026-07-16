CREATE OR REPLACE FUNCTION fn_camp_get_by_id(p_camp_id BIGINT)
RETURNS TABLE(campid BIGINT, centerid BIGINT, campcode VARCHAR, campname VARCHAR, organizerid BIGINT, venue VARCHAR, city VARCHAR, campdate DATE, totaldonorsexpected INTEGER, totaldonorscollected INTEGER, createdat TIMESTAMPTZ)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY SELECT c.CampId, c.CenterId, c.CampCode, c.CampName, c.OrganizerId, c.Venue, c.City, c.CampDate, c.TotalDonorsExpected, c.TotalDonorsCollected, c.CreatedAt
    FROM BloodCampMaster c WHERE c.CampId = p_camp_id;
END;
$$;
