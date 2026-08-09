-- ============================================================================
-- BloodCenterOS — Patch 20260809-017: Store Inventory Sign Fix (Patch 013 follow-up)
-- Description: Patch 013 stored outward transactions with a NEGATIVE TransQty
--   (-p_qty), but fn_report_inv_stock, fn_inv_trans_get_summary and the
--   outward stock-validation all treat 'O' rows as POSITIVE quantities.
--   This double-counted outward stock (10 in - 4 out showed 14 instead of 6).
--
-- Fix: store outward TransQty as a positive value ('O' type already
--   identifies direction). No deployment had Patch 013 applied, so no legacy
--   negative rows exist.
-- Apply: & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -d bloodcenter -f patch_20260809_017_store_inventory_fix.sql
-- ============================================================================

-- 1. Outward: store positive quantity -----------------------------------------
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
    VALUES (p_center_id, p_item_id, p_qty, 'O', NOW(), p_desc, p_created_by)
    RETURNING TransId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 2. Summary: OutwardQty must be positive magnitude ----------------------------
CREATE OR REPLACE FUNCTION fn_inv_trans_get_summary(p_center_id BIGINT)
RETURNS TABLE(ItemId BIGINT, ItemName VARCHAR, ItemUnit VARCHAR, MinOrderQty INT, InwardQty BIGINT, OutwardQty BIGINT, CurrentStock BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT i.ItemId, i.ItemName, i.ItemUnit, i.MinOrderQty,
        COALESCE(SUM(CASE WHEN t.TransTyp = 'I' THEN t.TransQty ELSE 0 END), 0)::BIGINT AS InwardQty,
        COALESCE(SUM(CASE WHEN t.TransTyp = 'O' THEN t.TransQty ELSE 0 END), 0)::BIGINT AS OutwardQty,
        COALESCE(SUM(CASE WHEN t.TransTyp = 'I' THEN t.TransQty ELSE -t.TransQty END), 0)::BIGINT AS CurrentStock
    FROM InvItems i LEFT JOIN InvTrans t ON t.ItemId = i.ItemId AND t.CenterId = p_center_id
    WHERE i.CenterId = p_center_id AND i.IsActive = TRUE
    GROUP BY i.ItemId, i.ItemName, i.ItemUnit, i.MinOrderQty
    ORDER BY i.ItemName;
END;
$$ LANGUAGE plpgsql;
