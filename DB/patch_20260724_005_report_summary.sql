-- ============================================================================
-- BloodCenterOS — Patch 20260724-005: Report Summary SPs TIMESTAMP fix
-- Description: Change fn_report_donor_summary / fn_report_camp_summary
--   param types from DATE to TIMESTAMP to match Npgsql DateTime mapping.
--   Fixes Npgsql.PostgresException 42883 (function does not exist).
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_005_report_summary.sql
-- ============================================================================

-- Fix fn_report_donor_summary: TIMESTAMP params instead of DATE
CREATE OR REPLACE FUNCTION fn_report_donor_summary(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP
) RETURNS TABLE(period VARCHAR, total_registered BIGINT,
    total_blood_group_a_positive BIGINT, total_blood_group_a_negative BIGINT,
    total_blood_group_b_positive BIGINT, total_blood_group_b_negative BIGINT,
    total_blood_group_ab_positive BIGINT, total_blood_group_ab_negative BIGINT,
    total_blood_group_o_positive BIGINT, total_blood_group_o_negative BIGINT,
    total_deferrals BIGINT, total_collections BIGINT) AS $$
BEGIN
    RETURN QUERY
    SELECT
        TO_CHAR(d.CreatedAt, 'YYYY-MM')::VARCHAR AS period,
        COUNT(DISTINCT d.DonorId)::BIGINT AS total_registered,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'A+' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'A-' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'B+' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'B-' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'AB+' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'AB-' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'O+' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT CASE WHEN d.BloodGroup = 'O-' THEN d.DonorId END)::BIGINT,
        COUNT(DISTINCT df.DeferralId)::BIGINT AS total_deferrals,
        COUNT(DISTINCT c.CollectionId)::BIGINT AS total_collections
    FROM DonorMaster d
    LEFT JOIN DeferralRecord df ON df.DonorId = d.DonorId
        AND df.CenterId = d.CenterId
        AND df.DeferralDate::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    LEFT JOIN CollectionRecord c ON c.DonorId = d.DonorId
        AND c.CenterId = d.CenterId
        AND c.CreatedAt::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    WHERE d.CenterId = p_center_id
        AND d.CreatedAt::DATE BETWEEN p_from_date::DATE AND p_to_date::DATE
    GROUP BY period
    ORDER BY period;
END;
$$ LANGUAGE plpgsql;

-- Fix fn_report_camp_summary: TIMESTAMP params instead of DATE
CREATE OR REPLACE FUNCTION fn_report_camp_summary(
    p_center_id BIGINT, p_from_date TIMESTAMP, p_to_date TIMESTAMP
) RETURNS TABLE(period VARCHAR, total_camps BIGINT,
    total_expected BIGINT, total_collected BIGINT, collection_rate NUMERIC) AS $$
BEGIN
    RETURN QUERY
    SELECT
        TO_CHAR(c.CampDate, 'YYYY-MM')::VARCHAR AS period,
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
        AND c.CampDate BETWEEN p_from_date::DATE AND p_to_date::DATE
    GROUP BY period
    ORDER BY period;
END;
$$ LANGUAGE plpgsql;
