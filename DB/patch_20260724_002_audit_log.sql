-- ============================================================================
-- BloodCenterOS — Patch 20260724-002: Audit Log Infrastructure
-- Description: AuditLog table + fn_audit_log SP for automatic data-change tracking
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_002_audit_log.sql
-- ============================================================================

-- 1. Table (if not exists) ---------------------------------------------------

CREATE TABLE IF NOT EXISTS AuditLog (
    AuditLogId        BIGSERIAL       NOT NULL,
    PropertyOwnerId   BIGINT          NOT NULL,
    UserId            BIGINT          NOT NULL,
    Action            VARCHAR(100)    NOT NULL,
    TableName         VARCHAR(200),
    RecordId          VARCHAR(100),
    ActionDetails     VARCHAR(4000),
    OldValue          VARCHAR(4000),
    NewValue          VARCHAR(4000),
    IpAddress         VARCHAR(50),
    UserAgent         VARCHAR(500),
    CreatedAt         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    PRIMARY KEY (AuditLogId)
);

-- 2. Stored Procedure --------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_audit_log(
    p_property_owner_id BIGINT, p_user_id BIGINT, p_action VARCHAR,
    p_table_name VARCHAR, p_record_id VARCHAR, p_details VARCHAR,
    p_old_val VARCHAR, p_new_val VARCHAR, p_ip VARCHAR, p_agent VARCHAR
) RETURNS VOID AS $$
BEGIN
    INSERT INTO AuditLog (PropertyOwnerId, UserId, Action, TableName, RecordId,
        ActionDetails, OldValue, NewValue, IpAddress, UserAgent, CreatedAt)
    VALUES (p_property_owner_id, p_user_id, p_action, p_table_name, p_record_id,
        p_details, p_old_val, p_new_val, p_ip, p_agent, NOW());
END;
$$ LANGUAGE plpgsql;
