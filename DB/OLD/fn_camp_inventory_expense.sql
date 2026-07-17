-- ============================================================================
-- Stored Procedures: CampInventory, CampExpenseLog
-- ============================================================================

DROP FUNCTION IF EXISTS fn_camp_inventory_get_by_camp(BIGINT);
DROP FUNCTION IF EXISTS fn_camp_inventory_get_by_center(BIGINT);
DROP FUNCTION IF EXISTS fn_camp_expense_get_by_camp(BIGINT);
DROP FUNCTION IF EXISTS fn_camp_expense_get_by_center(BIGINT);

-- ── CampInventory ──
CREATE OR REPLACE FUNCTION fn_camp_inventory_create(
    p_camp_id BIGINT, p_item_name VARCHAR, p_quantity INT, p_unit VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CampInventory (CampId, ItemName, Quantity, Unit, CreatedAt)
    VALUES (p_camp_id, p_item_name, p_quantity, p_unit, NOW())
    RETURNING CampInventoryId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_inventory_update(
    p_inventory_id BIGINT, p_item_name VARCHAR, p_quantity INT, p_unit VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE CampInventory SET
        ItemName = COALESCE(p_item_name, ItemName),
        Quantity = COALESCE(p_quantity, Quantity),
        Unit = COALESCE(p_unit, Unit)
    WHERE CampInventoryId = p_inventory_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_inventory_get_by_camp(p_camp_id BIGINT)
RETURNS TABLE(CampInventoryId BIGINT, CampId BIGINT, ItemName VARCHAR, Quantity INT, Unit VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT ci.CampInventoryId, ci.CampId, ci.ItemName, ci.Quantity, ci.Unit, ci.CreatedAt
    FROM CampInventory ci WHERE ci.CampId = p_camp_id ORDER BY ci.ItemName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_inventory_get_by_center(p_center_id BIGINT)
RETURNS TABLE(CampInventoryId BIGINT, CampId BIGINT, CampName VARCHAR, ItemName VARCHAR, Quantity INT, Unit VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT ci.CampInventoryId, ci.CampId, b.CampName, ci.ItemName, ci.Quantity, ci.Unit, ci.CreatedAt
    FROM CampInventory ci JOIN BloodCampMaster b ON b.CampId = ci.CampId
    WHERE b.CenterId = p_center_id ORDER BY b.CampName, ci.ItemName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_inventory_delete(p_inventory_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM CampInventory WHERE CampInventoryId = p_inventory_id;
END;
$$ LANGUAGE plpgsql;

-- ── CampExpenseLog ──
CREATE OR REPLACE FUNCTION fn_camp_expense_create(
    p_camp_id BIGINT, p_category VARCHAR, p_amount NUMERIC, p_notes VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CampExpenseLog (CampId, ExpenseCategory, Amount, Notes, CreatedAt)
    VALUES (p_camp_id, p_category, p_amount, p_notes, NOW())
    RETURNING CampExpenseId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_expense_update(
    p_expense_id BIGINT, p_category VARCHAR, p_amount NUMERIC, p_notes VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE CampExpenseLog SET
        ExpenseCategory = COALESCE(p_category, ExpenseCategory),
        Amount = COALESCE(p_amount, Amount),
        Notes = COALESCE(p_notes, Notes)
    WHERE CampExpenseId = p_expense_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_expense_get_by_camp(p_camp_id BIGINT)
RETURNS TABLE(CampExpenseId BIGINT, CampId BIGINT, ExpenseCategory VARCHAR, Amount NUMERIC, Notes VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT ce.CampExpenseId, ce.CampId, ce.ExpenseCategory, ce.Amount, ce.Notes, ce.CreatedAt
    FROM CampExpenseLog ce WHERE ce.CampId = p_camp_id ORDER BY ce.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_expense_get_by_center(p_center_id BIGINT)
RETURNS TABLE(CampExpenseId BIGINT, CampId BIGINT, CampName VARCHAR, ExpenseCategory VARCHAR, Amount NUMERIC, Notes VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT ce.CampExpenseId, ce.CampId, b.CampName, ce.ExpenseCategory, ce.Amount, ce.Notes, ce.CreatedAt
    FROM CampExpenseLog ce JOIN BloodCampMaster b ON b.CampId = ce.CampId
    WHERE b.CenterId = p_center_id ORDER BY ce.CreatedAt DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_expense_delete(p_expense_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM CampExpenseLog WHERE CampExpenseId = p_expense_id;
END;
$$ LANGUAGE plpgsql;
