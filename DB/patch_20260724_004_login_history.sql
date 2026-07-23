-- ============================================================================
-- BloodCenterOS — Patch 20260724-004: Login History
-- Description: LoginHistory table + SPs for tracking user sessions
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_004_login_history.sql
-- ============================================================================

-- 1. Table (if not exists) ---------------------------------------------------

CREATE TABLE IF NOT EXISTS LoginHistory (
    LoginHistoryId  BIGSERIAL       NOT NULL,
    UserId          BIGINT          NOT NULL,
    CenterId        BIGINT,
    LoginAt         TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    LogoutAt        TIMESTAMPTZ,
    IpAddress       VARCHAR(50),
    UserAgent       VARCHAR(500),
    PRIMARY KEY (LoginHistoryId)
);

-- 2. Stored Procedures -------------------------------------------------------

CREATE OR REPLACE FUNCTION fn_login_history_create(
    p_user_id BIGINT, p_center_id BIGINT, p_ip VARCHAR, p_agent VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO LoginHistory (UserId, CenterId, LoginAt, IpAddress, UserAgent)
    VALUES (p_user_id, p_center_id, NOW(), p_ip, p_agent)
    RETURNING LoginHistoryId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_login_history_logout(p_login_id BIGINT)
RETURNS VOID AS $$
BEGIN
    UPDATE LoginHistory SET LogoutAt = NOW() WHERE LoginHistoryId = p_login_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_login_history_get_filtered(
    p_user_id BIGINT DEFAULT NULL, p_from_date TIMESTAMPTZ DEFAULT NULL,
    p_to_date TIMESTAMPTZ DEFAULT NULL, p_limit INT DEFAULT 200
) RETURNS TABLE(
    loginhistoryid BIGINT, userid BIGINT, username VARCHAR, displayname VARCHAR,
    centerid BIGINT, loginat TIMESTAMPTZ, logoutat TIMESTAMPTZ,
    ipaddress VARCHAR, useragent VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT lh.LoginHistoryId, lh.UserId, u.UserName, u.DisplayName,
        lh.CenterId, lh.LoginAt, lh.LogoutAt, lh.IpAddress, lh.UserAgent
    FROM LoginHistory lh
    JOIN UserMaster u ON u.UserId = lh.UserId
    WHERE (p_user_id IS NULL OR lh.UserId = p_user_id)
      AND (p_from_date IS NULL OR lh.LoginAt >= p_from_date)
      AND (p_to_date IS NULL OR lh.LoginAt <= p_to_date)
    ORDER BY lh.LoginAt DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;
