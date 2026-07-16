-- ============================================================================
-- Stored Procedures: ComponentTypeMaster
-- ============================================================================
CREATE OR REPLACE FUNCTION fn_component_type_create(p_code VARCHAR, p_desc VARCHAR)
RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ComponentTypeMaster (ComponentTypeCode, Description, CreatedAt)
    VALUES (p_code, p_desc, NOW())
    RETURNING ComponentTypeId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_component_type_update(p_id BIGINT, p_code VARCHAR, p_desc VARCHAR)
RETURNS VOID AS $$
BEGIN
    UPDATE ComponentTypeMaster SET
        ComponentTypeCode = COALESCE(p_code, ComponentTypeCode),
        Description = COALESCE(p_desc, Description)
    WHERE ComponentTypeId = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_component_type_get_all()
RETURNS TABLE(ComponentTypeId BIGINT, ComponentTypeCode VARCHAR, Description VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT c.ComponentTypeId, c.ComponentTypeCode, c.Description, c.CreatedAt
    FROM ComponentTypeMaster c ORDER BY c.ComponentTypeCode;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_component_type_get_by_id(p_id BIGINT)
RETURNS TABLE(ComponentTypeId BIGINT, ComponentTypeCode VARCHAR, Description VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT c.ComponentTypeId, c.ComponentTypeCode, c.Description, c.CreatedAt
    FROM ComponentTypeMaster c WHERE c.ComponentTypeId = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_component_type_delete(p_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM ComponentTypeMaster WHERE ComponentTypeId = p_id;
END;
$$ LANGUAGE plpgsql;
