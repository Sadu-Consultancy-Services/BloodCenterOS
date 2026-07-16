CREATE OR REPLACE FUNCTION fn_center_config_get_all(p_center_id BIGINT)
RETURNS TABLE(ConfigKey VARCHAR, ConfigValue TEXT) AS $$
BEGIN
    RETURN QUERY SELECT c.ConfigKey, c.ConfigValue
    FROM CenterConfig c WHERE c.CenterId = p_center_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_system_config_get_all(p_center_id BIGINT)
RETURNS TABLE(ConfigKey VARCHAR, ConfigValue TEXT, Description VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT sc.ConfigKey, sc.ConfigValue, sc.Description
    FROM SystemConfig sc WHERE p_center_id IS NULL OR sc.CenterId = p_center_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_lookup_type_get_all()
RETURNS TABLE(LookupTypeId BIGINT, TypeCode VARCHAR, TypeName VARCHAR, Description VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT lt.LookupTypeId, lt.TypeCode, lt.TypeName, lt.Description
    FROM LookupType lt ORDER BY lt.TypeCode;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_lookup_value_get_all(
    p_type_id BIGINT, p_center_id BIGINT
) RETURNS TABLE(
    LookupValueId BIGINT, LookupTypeId BIGINT, ValueCode VARCHAR,
    ValueText VARCHAR, SortOrder INT, IsActive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY SELECT lv.LookupValueId, lv.LookupTypeId, lv.ValueCode,
        lv.ValueText, lv.SortOrder, lv.IsActive
    FROM LookupValue lv
    WHERE lv.LookupTypeId = p_type_id
        AND (p_center_id IS NULL OR lv.CenterId = p_center_id)
    ORDER BY lv.SortOrder;
END;
$$ LANGUAGE plpgsql;
