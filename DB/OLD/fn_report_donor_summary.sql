CREATE OR REPLACE FUNCTION fn_report_donor_summary(
    p_center_id BIGINT, p_from_date DATE, p_to_date DATE
) RETURNS TABLE(
    period VARCHAR, total_registered BIGINT,
    total_blood_group_a_positive BIGINT, total_blood_group_a_negative BIGINT,
    total_blood_group_b_positive BIGINT, total_blood_group_b_negative BIGINT,
    total_blood_group_ab_positive BIGINT, total_blood_group_ab_negative BIGINT,
    total_blood_group_o_positive BIGINT, total_blood_group_o_negative BIGINT,
    total_deferrals BIGINT, total_collections BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        TO_CHAR(d.CreatedAt, 'YYYY-MM') AS period,
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
        AND df.DeferralDate::DATE BETWEEN p_from_date AND p_to_date
    LEFT JOIN CollectionRecord c ON c.DonorId = d.DonorId
        AND c.CenterId = d.CenterId
        AND c.CreatedAt::DATE BETWEEN p_from_date AND p_to_date
    WHERE d.CenterId = p_center_id
        AND d.CreatedAt::DATE BETWEEN p_from_date AND p_to_date
    GROUP BY period
    ORDER BY period;
END;
$$ LANGUAGE plpgsql;
