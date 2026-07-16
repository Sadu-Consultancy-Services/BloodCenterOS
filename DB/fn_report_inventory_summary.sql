CREATE OR REPLACE FUNCTION fn_report_inventory_summary(
    p_center_id BIGINT
) RETURNS TABLE(
    component_type VARCHAR, blood_group VARCHAR,
    available_qty BIGINT, reserved_qty BIGINT,
    quarantined_qty BIGINT,
    near_expiry_qty BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COALESCE(s.ComponentType, '') AS component_type,
        COALESCE(s.BloodGroup, '') AS blood_group,
        COALESCE(s.AvailableQty, 0)::BIGINT AS available_qty,
        COALESCE(s.ReservedQty, 0)::BIGINT AS reserved_qty,
        COALESCE(s.QuarantinedQty, 0)::BIGINT AS quarantined_qty,
        COUNT(cm.ComponentId)::BIGINT AS near_expiry_qty
    FROM InventoryStock s
    LEFT JOIN ComponentMaster cm ON cm.CenterId = s.CenterId
        AND cm.ComponentType = s.ComponentType
        AND cm.CurrentStatus IN ('Available', 'Quarantined')
        AND cm.ExpiryDate IS NOT NULL
        AND cm.ExpiryDate BETWEEN CURRENT_DATE AND (CURRENT_DATE + INTERVAL '30 days')
    WHERE s.CenterId = p_center_id
    GROUP BY s.ComponentType, s.BloodGroup, s.AvailableQty, s.ReservedQty, s.QuarantinedQty
    ORDER BY s.ComponentType, s.BloodGroup;
END;
$$ LANGUAGE plpgsql;
