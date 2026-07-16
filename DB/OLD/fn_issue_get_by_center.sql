CREATE OR REPLACE FUNCTION fn_issue_get_by_center(p_center_id BIGINT)
RETURNS TABLE(IssueRecordId BIGINT, CenterId BIGINT, ComponentId BIGINT, BagId BIGINT,
    PatientName VARCHAR, HospitalId BIGINT, IssueDate TIMESTAMPTZ, IssuedByUserId BIGINT,
    IssueType VARCHAR, IssueSlipNumber VARCHAR, Notes VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT i.IssueRecordId, i.CenterId, i.ComponentId, i.BagId,
        i.PatientName, i.HospitalId, i.IssueDate, i.IssuedByUserId,
        i.IssueType, i.IssueSlipNumber, i.Notes
    FROM IssueRecord i
    WHERE i.CenterId = p_center_id
    ORDER BY i.IssueDate DESC;
END;
$$ LANGUAGE plpgsql;
