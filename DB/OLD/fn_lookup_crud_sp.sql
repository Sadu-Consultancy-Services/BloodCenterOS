CREATE OR REPLACE FUNCTION fn_lookup_type_create(
    p_code VARCHAR, p_name VARCHAR, p_desc VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO LookupType (TypeCode, TypeName, Description, CreatedAt)
    VALUES (p_code, p_name, p_desc, NOW()) RETURNING LookupTypeId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_lookup_value_create(
    p_type_id BIGINT, p_center_id BIGINT, p_code VARCHAR,
    p_text VARCHAR, p_sort INT, p_active BOOLEAN DEFAULT TRUE
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO LookupValue (LookupTypeId, CenterId, ValueCode, ValueText, SortOrder, IsActive, CreatedAt)
    VALUES (p_type_id, p_center_id, p_code, p_text, p_sort, p_active, NOW())
    RETURNING LookupValueId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;
