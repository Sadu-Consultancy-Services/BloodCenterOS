CREATE OR REPLACE FUNCTION fn_inventory_get_stock(p_center_id BIGINT)
 RETURNS TABLE(inventorystockid BIGINT, centerid BIGINT, componenttype VARCHAR,
    bloodgroup VARCHAR, availableqty INTEGER, reservedqty INTEGER,
    quarantinedqty INTEGER, lastupdatedat TIMESTAMPTZ, lastupdatedby BIGINT,
    createdat TIMESTAMPTZ)
 LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY SELECT s.inventorystockid, s.centerid, s.componenttype, s.bloodgroup,
        s.availableqty, s.reservedqty, s.quarantinedqty, s.lastupdatedat,
        s.lastupdatedby, s.createdat
    FROM InventoryStock s
    WHERE s.CenterId = p_center_id AND s.AvailableQty > 0
    ORDER BY s.BloodGroup, s.ComponentType;
END;
$$;
