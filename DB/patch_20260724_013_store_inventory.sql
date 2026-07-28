-- ============================================================================
-- BloodCenterOS — Patch 20260724-013: Store Inventory Management
-- Description: Non-blood inventory (reagents, consumables). Items master,
--   inward/outward transactions with stock validation.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260724_013_store_inventory.sql
-- ============================================================================

-- 1. InvItems Table ----------------------------------------------------------

CREATE TABLE IF NOT EXISTS InvItems (
    ItemId       BIGSERIAL PRIMARY KEY,
    CenterId     BIGINT NOT NULL DEFAULT 0,
    ItemName     VARCHAR(300) NOT NULL,
    MinOrderQty  INT NOT NULL DEFAULT 0,
    ItemUnit     VARCHAR(50),
    IsActive     BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedBy    BIGINT,
    CreatedAt    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. InvTrans Table ----------------------------------------------------------

CREATE TABLE IF NOT EXISTS InvTrans (
    TransId     BIGSERIAL PRIMARY KEY,
    CenterId    BIGINT NOT NULL DEFAULT 0,
    ItemId      BIGINT NOT NULL REFERENCES InvItems(ItemId),
    TransQty    INT NOT NULL,
    TransTyp    VARCHAR(1) NOT NULL CHECK (TransTyp IN ('I','O')),  -- I=Inward, O=Outward
    TransDate   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    TransDesc   VARCHAR(500),
    CreatedBy   BIGINT,
    CreatedAt   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3. InvItems SPs ------------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_inv_items_get_by_center(p_center_id BIGINT)
RETURNS TABLE(ItemId BIGINT, ItemName VARCHAR, MinOrderQty INT, ItemUnit VARCHAR, IsActive BOOLEAN, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT i.ItemId, i.ItemName, i.MinOrderQty, i.ItemUnit, i.IsActive, i.CreatedAt
    FROM InvItems i WHERE i.CenterId = p_center_id ORDER BY i.ItemName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inv_items_get_active(p_center_id BIGINT)
RETURNS TABLE(ItemId BIGINT, ItemName VARCHAR, MinOrderQty INT, ItemUnit VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT i.ItemId, i.ItemName, i.MinOrderQty, i.ItemUnit
    FROM InvItems i WHERE i.CenterId = p_center_id AND i.IsActive = TRUE ORDER BY i.ItemName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inv_items_get_by_id(p_id BIGINT)
RETURNS TABLE(ItemId BIGINT, CenterId BIGINT, ItemName VARCHAR, MinOrderQty INT, ItemUnit VARCHAR, IsActive BOOLEAN) AS $$
BEGIN
    RETURN QUERY SELECT i.ItemId, i.CenterId, i.ItemName, i.MinOrderQty, i.ItemUnit, i.IsActive
    FROM InvItems i WHERE i.ItemId = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inv_items_upsert(
    p_center_id BIGINT, p_id BIGINT, p_name VARCHAR, p_min_qty INT, p_unit VARCHAR, p_is_active BOOLEAN, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    IF p_id > 0 AND EXISTS (SELECT 1 FROM InvItems WHERE ItemId = p_id) THEN
        UPDATE InvItems SET ItemName = p_name, MinOrderQty = p_min_qty, ItemUnit = p_unit, IsActive = p_is_active
        WHERE ItemId = p_id RETURNING ItemId INTO v_id;
        RETURN v_id;
    END IF;
    INSERT INTO InvItems (CenterId, ItemName, MinOrderQty, ItemUnit, IsActive, CreatedBy)
    VALUES (p_center_id, p_name, p_min_qty, p_unit, p_is_active, p_created_by)
    RETURNING ItemId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inv_items_delete(p_id BIGINT) RETURNS VOID AS $$
BEGIN
    UPDATE InvItems SET IsActive = FALSE WHERE ItemId = p_id;
END;
$$ LANGUAGE plpgsql;

-- 4. InvTrans SPs ------------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_inv_trans_inward(
    p_center_id BIGINT, p_item_id BIGINT, p_qty INT, p_desc VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_min_qty INT; v_id BIGINT;
BEGIN
    SELECT MinOrderQty INTO v_min_qty FROM InvItems WHERE ItemId = p_item_id AND IsActive = TRUE;
    IF NOT FOUND THEN RAISE EXCEPTION 'Item not found or inactive'; END IF;

    IF p_qty < v_min_qty THEN
        RAISE EXCEPTION 'Quantity (%) is less than minimum order quantity (%)', p_qty, v_min_qty;
    END IF;

    INSERT INTO InvTrans (CenterId, ItemId, TransQty, TransTyp, TransDate, TransDesc, CreatedBy)
    VALUES (p_center_id, p_item_id, p_qty, 'I', NOW(), p_desc, p_created_by)
    RETURNING TransId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inv_trans_outward(
    p_center_id BIGINT, p_item_id BIGINT, p_qty INT, p_desc VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_available INT; v_id BIGINT;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM InvItems WHERE ItemId = p_item_id AND IsActive = TRUE) THEN
        RAISE EXCEPTION 'Item not found or inactive';
    END IF;

    SELECT COALESCE(SUM(CASE WHEN TransTyp = 'I' THEN TransQty ELSE -TransQty END), 0)
    INTO v_available FROM InvTrans WHERE ItemId = p_item_id AND CenterId = p_center_id;

    IF p_qty > v_available THEN
        RAISE EXCEPTION 'Insufficient stock. Available: %, requested: %', v_available, p_qty;
    END IF;

    INSERT INTO InvTrans (CenterId, ItemId, TransQty, TransTyp, TransDate, TransDesc, CreatedBy)
    VALUES (p_center_id, p_item_id, -p_qty, 'O', NOW(), p_desc, p_created_by)
    RETURNING TransId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inv_trans_get_by_item(
    p_center_id BIGINT, p_item_id BIGINT, p_from_date DATE DEFAULT NULL, p_to_date DATE DEFAULT NULL
)
RETURNS TABLE(TransId BIGINT, ItemId BIGINT, ItemName VARCHAR, TransQty INT, TransTyp VARCHAR, TransDate TIMESTAMPTZ, TransDesc VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT t.TransId, i.ItemId, i.ItemName, t.TransQty, t.TransTyp, t.TransDate, t.TransDesc
    FROM InvTrans t JOIN InvItems i ON i.ItemId = t.ItemId
    WHERE t.CenterId = p_center_id AND t.ItemId = p_item_id
        AND (p_from_date IS NULL OR t.TransDate::DATE >= p_from_date)
        AND (p_to_date IS NULL OR t.TransDate::DATE <= p_to_date)
    ORDER BY t.TransDate DESC;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inv_trans_get_summary(p_center_id BIGINT)
RETURNS TABLE(ItemId BIGINT, ItemName VARCHAR, ItemUnit VARCHAR, MinOrderQty INT, InwardQty BIGINT, OutwardQty BIGINT, CurrentStock BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT i.ItemId, i.ItemName, i.ItemUnit, i.MinOrderQty,
        COALESCE(SUM(CASE WHEN t.TransTyp = 'I' THEN t.TransQty ELSE 0 END), 0)::BIGINT AS InwardQty,
        COALESCE(SUM(CASE WHEN t.TransTyp = 'O' THEN -t.TransQty ELSE 0 END), 0)::BIGINT AS OutwardQty,
        COALESCE(SUM(CASE WHEN t.TransTyp = 'I' THEN t.TransQty ELSE -t.TransQty END), 0)::BIGINT AS CurrentStock
    FROM InvItems i LEFT JOIN InvTrans t ON t.ItemId = i.ItemId AND t.CenterId = p_center_id
    WHERE i.CenterId = p_center_id AND i.IsActive = TRUE
    GROUP BY i.ItemId, i.ItemName, i.ItemUnit, i.MinOrderQty
    ORDER BY i.ItemName;
END;
$$ LANGUAGE plpgsql;
