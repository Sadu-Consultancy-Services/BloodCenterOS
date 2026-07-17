-- ============================================================================
-- Stored Procedures: DeviceMaster, FridgeStorageMaster
-- ============================================================================

-- ── DeviceMaster ──
CREATE OR REPLACE FUNCTION fn_device_create(
    p_center_id BIGINT, p_name VARCHAR, p_type VARCHAR,
    p_serial VARCHAR, p_purchase_date DATE, p_warranty_end DATE
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DeviceMaster (CenterId, DeviceName, DeviceType, SerialNumber, PurchaseDate, WarrantyEndDate, CreatedAt)
    VALUES (p_center_id, p_name, p_type, p_serial, p_purchase_date, p_warranty_end, NOW())
    RETURNING DeviceId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_device_update(
    p_device_id BIGINT, p_name VARCHAR, p_type VARCHAR,
    p_serial VARCHAR, p_purchase_date DATE, p_warranty_end DATE
) RETURNS VOID AS $$
BEGIN
    UPDATE DeviceMaster SET
        DeviceName = COALESCE(p_name, DeviceName),
        DeviceType = COALESCE(p_type, DeviceType),
        SerialNumber = COALESCE(p_serial, SerialNumber),
        PurchaseDate = COALESCE(p_purchase_date, PurchaseDate),
        WarrantyEndDate = COALESCE(p_warranty_end, WarrantyEndDate)
    WHERE DeviceId = p_device_id;
END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS fn_device_get_by_id(BIGINT);
CREATE OR REPLACE FUNCTION fn_device_get_by_id(p_device_id BIGINT)
RETURNS TABLE(DeviceId BIGINT, CenterId BIGINT, DeviceName VARCHAR, DeviceType VARCHAR,
    SerialNumber VARCHAR, PurchaseDate DATE, WarrantyEndDate DATE, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT d.DeviceId, d.CenterId, d.DeviceName, d.DeviceType,
        d.SerialNumber, d.PurchaseDate, d.WarrantyEndDate, d.CreatedAt
    FROM DeviceMaster d WHERE d.DeviceId = p_device_id;
END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS fn_device_get_by_center(BIGINT);
CREATE OR REPLACE FUNCTION fn_device_get_by_center(p_center_id BIGINT)
RETURNS TABLE(DeviceId BIGINT, CenterId BIGINT, DeviceName VARCHAR, DeviceType VARCHAR,
    SerialNumber VARCHAR, PurchaseDate DATE, WarrantyEndDate DATE, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT d.DeviceId, d.CenterId, d.DeviceName, d.DeviceType,
        d.SerialNumber, d.PurchaseDate, d.WarrantyEndDate, d.CreatedAt
    FROM DeviceMaster d WHERE d.CenterId = p_center_id ORDER BY d.DeviceName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_device_delete(p_device_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM DeviceMaster WHERE DeviceId = p_device_id;
END;
$$ LANGUAGE plpgsql;

-- ── FridgeStorageMaster ──
CREATE OR REPLACE FUNCTION fn_fridge_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR,
    p_capacity INT, p_location VARCHAR, p_temp_log BOOLEAN
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO FridgeStorageMaster (CenterId, FridgeCode, FridgeName, Capacity, Location, TemperatureLogRequired, CreatedAt)
    VALUES (p_center_id, p_code, p_name, p_capacity, p_location, p_temp_log, NOW())
    RETURNING FridgeId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_fridge_update(
    p_fridge_id BIGINT, p_code VARCHAR, p_name VARCHAR,
    p_capacity INT, p_location VARCHAR, p_temp_log BOOLEAN
) RETURNS VOID AS $$
BEGIN
    UPDATE FridgeStorageMaster SET
        FridgeCode = COALESCE(p_code, FridgeCode),
        FridgeName = COALESCE(p_name, FridgeName),
        Capacity = COALESCE(p_capacity, Capacity),
        Location = COALESCE(p_location, Location),
        TemperatureLogRequired = COALESCE(p_temp_log, TemperatureLogRequired)
    WHERE FridgeId = p_fridge_id;
END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS fn_fridge_get_by_id(BIGINT);
CREATE OR REPLACE FUNCTION fn_fridge_get_by_id(p_fridge_id BIGINT)
RETURNS TABLE(FridgeId BIGINT, CenterId BIGINT, FridgeCode VARCHAR, FridgeName VARCHAR,
    Capacity INT, Location VARCHAR, TemperatureLogRequired BOOLEAN, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT f.FridgeId, f.CenterId, f.FridgeCode, f.FridgeName,
        f.Capacity, f.Location, f.TemperatureLogRequired, f.CreatedAt
    FROM FridgeStorageMaster f WHERE f.FridgeId = p_fridge_id;
END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS fn_fridge_get_by_center(BIGINT);
CREATE OR REPLACE FUNCTION fn_fridge_get_by_center(p_center_id BIGINT)
RETURNS TABLE(FridgeId BIGINT, CenterId BIGINT, FridgeCode VARCHAR, FridgeName VARCHAR,
    Capacity INT, Location VARCHAR, TemperatureLogRequired BOOLEAN, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT f.FridgeId, f.CenterId, f.FridgeCode, f.FridgeName,
        f.Capacity, f.Location, f.TemperatureLogRequired, f.CreatedAt
    FROM FridgeStorageMaster f WHERE f.CenterId = p_center_id ORDER BY f.FridgeName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_fridge_delete(p_fridge_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM FridgeStorageMaster WHERE FridgeId = p_fridge_id;
END;
$$ LANGUAGE plpgsql;
