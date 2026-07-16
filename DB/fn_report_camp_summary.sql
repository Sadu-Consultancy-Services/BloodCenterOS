CREATE OR REPLACE FUNCTION fn_report_camp_summary(
    p_center_id BIGINT, p_from_date DATE, p_to_date DATE
) RETURNS TABLE(
    period VARCHAR, total_camps BIGINT,
    total_expected BIGINT, total_collected BIGINT,
    collection_rate NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        TO_CHAR(c.CampDate, 'YYYY-MM') AS period,
        COUNT(c.CampId)::BIGINT AS total_camps,
        COALESCE(SUM(c.TotalDonorsExpected), 0)::BIGINT AS total_expected,
        COALESCE(SUM(c.TotalDonorsCollected), 0)::BIGINT AS total_collected,
        CASE
            WHEN COALESCE(SUM(c.TotalDonorsExpected), 0) > 0
            THEN ROUND(COALESCE(SUM(c.TotalDonorsCollected), 0)::NUMERIC / SUM(c.TotalDonorsExpected) * 100, 1)
            ELSE 0
        END AS collection_rate
    FROM BloodCampMaster c
    WHERE c.CenterId = p_center_id
        AND c.CampDate BETWEEN p_from_date AND p_to_date
    GROUP BY period
    ORDER BY period;
END;
$$ LANGUAGE plpgsql;
