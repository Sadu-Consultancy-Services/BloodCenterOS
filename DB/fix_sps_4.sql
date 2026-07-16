DROP FUNCTION IF EXISTS fn_donor_donation_create(BIGINT, BIGINT, BIGINT, VARCHAR, INTEGER, VARCHAR, VARCHAR, BIGINT);

CREATE OR REPLACE FUNCTION fn_donor_donation_create(
    p_center_id BIGINT, p_donor_id BIGINT, p_collection_id BIGINT,
    p_donation_type VARCHAR, p_volume NUMERIC, p_bag_no VARCHAR,
    p_remarks VARCHAR, p_created_by BIGINT
) RETURNS BIGINT LANGUAGE plpgsql AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DonorDonationHistory (CenterId, DonorId, CollectionId, DonationDate,
        DonationType, VolumeMl, BagNumber, Remarks, CreatedBy)
    VALUES (p_center_id, p_donor_id, p_collection_id, NOW(), p_donation_type,
        p_volume, p_bag_no, p_remarks, p_created_by)
    RETURNING DonationId INTO v_id;

    PERFORM fn_donor_update_donation_stats(p_donor_id);
    RETURN v_id;
END;
$$;
