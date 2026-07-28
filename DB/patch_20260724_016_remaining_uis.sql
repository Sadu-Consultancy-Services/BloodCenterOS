-- ============================================================================
-- BloodCenterOS — Patch 20260724-016: Remaining Web UI SPs
-- Description: List/get SPs for Notification, ReplacementDonor, BloodBag
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_016_remaining_uis.sql
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_notification_get_all(p_center_id BIGINT)
RETURNS TABLE(NotificationId BIGINT, NotificationType VARCHAR, Title VARCHAR,
    Body TEXT, TargetAudience VARCHAR, IsActive BOOLEAN, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT n.NotificationId, n.NotificationType::VARCHAR, n.Title::VARCHAR,
        n.Body, n.TargetAudience::VARCHAR, n.IsActive, n.CreatedAt
    FROM NotificationMaster n
    WHERE n.CenterId = p_center_id
    ORDER BY n.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_replacement_donor_get_all(p_center_id BIGINT)
RETURNS TABLE(ReplacementDonorId BIGINT, PatientRequestId BIGINT,
    DonorId BIGINT, DonorName VARCHAR, PatientName VARCHAR,
    DonatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT rd.ReplacementDonorId, rd.PatientRequestId, rd.DonorId,
        COALESCE(d.FirstName || ' ' || COALESCE(d.LastName, ''), '')::VARCHAR AS DonorName,
        COALESCE(pr.PatientName, '')::VARCHAR AS PatientName,
        rd.DonatedAt
    FROM ReplacementDonor rd
    LEFT JOIN DonorMaster d ON d.DonorId = rd.DonorId
    LEFT JOIN PatientRequest pr ON pr.RequestId = rd.PatientRequestId
    WHERE rd.CenterId = p_center_id
    ORDER BY rd.DonatedAt DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_blood_bag_search(
    p_center_id BIGINT, p_term VARCHAR DEFAULT NULL
) RETURNS TABLE(BagId BIGINT, BloodBagNumber VARCHAR, DonorName VARCHAR,
    BloodGroup VARCHAR, BagType VARCHAR, BagStatus VARCHAR,
    InitialCollectedAt TIMESTAMPTZ, ExpiryDate DATE) AS $$
BEGIN
    RETURN QUERY SELECT b.BagId, b.BloodBagNumber::VARCHAR,
        COALESCE(d.FirstName || ' ' || COALESCE(d.LastName, ''), '')::VARCHAR AS DonorName,
        COALESCE(d.BloodGroup, '')::VARCHAR, COALESCE(b.BagType, '')::VARCHAR,
        COALESCE(b.BagStatus, '')::VARCHAR, b.InitialCollectedAt, b.ExpiryDate
    FROM BloodBagMaster b
    LEFT JOIN DonorMaster d ON d.DonorId = b.DonorId
    WHERE b.CenterId = p_center_id
        AND (p_term IS NULL OR b.BloodBagNumber ILIKE '%' || p_term || '%'
            OR b.BagBarcode ILIKE '%' || p_term || '%')
    ORDER BY b.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;
