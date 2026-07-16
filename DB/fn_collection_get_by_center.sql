CREATE OR REPLACE FUNCTION fn_collection_get_by_center(p_center_id BIGINT)
RETURNS TABLE(CollectionId BIGINT, CenterId BIGINT, BranchId BIGINT, CampId BIGINT, DonorId BIGINT,
    BloodBagNumber VARCHAR, BagBarcode VARCHAR, BagLotNumber VARCHAR, BagVolumeMl INT,
    CollectorEmployeeId BIGINT, CollectionLocationType VARCHAR, CollectionStartTime TIMESTAMPTZ,
    CollectionEndTime TIMESTAMPTZ, Notes VARCHAR, CreatedAt TIMESTAMPTZ, CreatedBy BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT c.CollectionId, c.CenterId, c.BranchId, c.CampId, c.DonorId,
        c.BloodBagNumber, c.BagBarcode, c.BagLotNumber, c.BagVolumeMl,
        c.CollectorEmployeeId, c.CollectionLocationType, c.CollectionStartTime,
        c.CollectionEndTime, c.Notes, c.CreatedAt, c.CreatedBy
    FROM CollectionRecord c
    WHERE c.CenterId = p_center_id
    ORDER BY c.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;
