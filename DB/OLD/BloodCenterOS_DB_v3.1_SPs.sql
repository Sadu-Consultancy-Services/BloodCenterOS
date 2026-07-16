-- ============================================================================
-- BloodCenterOS — PL/pgSQL Stored Procedures
-- Version: 3.1
-- Description: All CRUD + business logic functions for 89 tables
-- Pattern: Stored-procedure-only data access (no ORM)
-- Convention: fn_<module>_<action> — PostgreSQL functions
-- ============================================================================

-- ============================================================================
-- 0. Helper Functions
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_sequence_next(p_center_id BIGINT, p_seq_name VARCHAR)
RETURNS VARCHAR AS $$
DECLARE
    v_prefix VARCHAR(20);
    v_suffix VARCHAR(20);
    v_next BIGINT;
    v_inc INTEGER;
    v_result VARCHAR(100);
BEGIN
    SELECT Prefix, Suffix, IncrementBy
    INTO v_prefix, v_suffix, v_inc
    FROM SequenceCounters
    WHERE CenterId = p_center_id AND SequenceName = p_seq_name;

    UPDATE SequenceCounters
    SET LastValue = LastValue + COALESCE(v_inc, 1),
        UpdatedAt = NOW()
    WHERE CenterId = p_center_id AND SequenceName = p_seq_name
    RETURNING LastValue INTO v_next;

    v_result := COALESCE(v_prefix, '') || LPAD(v_next::TEXT, 6, '0') || COALESCE(v_suffix, '');
    RETURN v_result;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_error_log(p_center_id BIGINT, p_message TEXT, p_stack TEXT)
RETURNS VOID AS $$
BEGIN
    INSERT INTO ErrorLog (CenterId, ErrorMessage, StackTrace, OccurredAt)
    VALUES (p_center_id, p_message, p_stack, NOW());
END;
$$ LANGUAGE plpgsql;

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

CREATE OR REPLACE FUNCTION fn_change_log(
    p_center_id BIGINT, p_entity VARCHAR, p_entity_id VARCHAR,
    p_change_type VARCHAR, p_change_data TEXT, p_changed_by BIGINT
) RETURNS VOID AS $$
BEGIN
    INSERT INTO ChangeLog (CenterId, EntityName, EntityId, ChangeType,
        ChangeData, ChangedBy, ChangedAt)
    VALUES (p_center_id, p_entity, p_entity_id, p_change_type,
        p_change_data, p_changed_by, NOW());
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 1. User & Access Management
-- ============================================================================

-- 1a. UserMaster
CREATE OR REPLACE FUNCTION fn_user_create(
    p_center_id BIGINT, p_username VARCHAR, p_display_name VARCHAR,
    p_email VARCHAR, p_phone VARCHAR, p_password_hash VARCHAR,
    p_password_salt VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE
    v_user_id BIGINT;
BEGIN
    INSERT INTO UserMaster (CenterId, UserName, DisplayName, Email, Phone,
        PasswordHash, PasswordSalt, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_username, p_display_name, p_email, p_phone,
        p_password_hash, p_password_salt, NOW(), p_created_by)
    RETURNING UserId INTO v_user_id;

    RETURN v_user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_get_by_id(p_user_id BIGINT)
RETURNS TABLE(UserId BIGINT, CenterId BIGINT, UserName VARCHAR, DisplayName VARCHAR,
    Email VARCHAR, Phone VARCHAR, IsLocked BOOLEAN, LastLoginAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ, CreatedBy BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT u.UserId, u.CenterId, u.UserName, u.DisplayName,
        u.Email, u.Phone, u.IsLocked, u.LastLoginAt, u.CreatedAt, u.CreatedBy
    FROM UserMaster u WHERE u.UserId = p_user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_get_by_username(p_username VARCHAR)
RETURNS TABLE(UserId BIGINT, CenterId BIGINT, UserName VARCHAR, DisplayName VARCHAR,
    Email VARCHAR, Phone VARCHAR, PasswordHash VARCHAR, PasswordSalt VARCHAR,
    IsLocked BOOLEAN, LastLoginAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT u.UserId, u.CenterId, u.UserName, u.DisplayName,
        u.Email, u.Phone, u.PasswordHash, u.PasswordSalt,
        u.IsLocked, u.LastLoginAt
    FROM UserMaster u WHERE u.UserName = p_username;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_update(
    p_user_id BIGINT, p_display_name VARCHAR, p_email VARCHAR,
    p_phone VARCHAR, p_updated_by BIGINT
) RETURNS VOID AS $$
BEGIN
    UPDATE UserMaster
    SET DisplayName = COALESCE(p_display_name, DisplayName),
        Email = COALESCE(p_email, Email),
        Phone = COALESCE(p_phone, Phone),
        UpdatedAt = NOW(),
        UpdatedBy = p_updated_by
    WHERE UserId = p_user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_update_password(
    p_user_id BIGINT, p_hash VARCHAR, p_salt VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE UserMaster
    SET PasswordHash = p_hash, PasswordSalt = p_salt, UpdatedAt = NOW()
    WHERE UserId = p_user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_toggle_lock(p_user_id BIGINT, p_lock BOOLEAN)
RETURNS VOID AS $$
BEGIN
    UPDATE UserMaster SET IsLocked = p_lock, UpdatedAt = NOW() WHERE UserId = p_user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_update_login(p_user_id BIGINT)
RETURNS VOID AS $$
BEGIN
    UPDATE UserMaster SET LastLoginAt = NOW() WHERE UserId = p_user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_search(
    p_center_id BIGINT, p_keyword VARCHAR DEFAULT NULL,
    p_page INT DEFAULT 1, p_size INT DEFAULT 20
) RETURNS TABLE(UserId BIGINT, UserName VARCHAR, DisplayName VARCHAR,
    Email VARCHAR, Phone VARCHAR, IsLocked BOOLEAN, LastLoginAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ, TotalCount BIGINT) AS $$
DECLARE
    v_offset INT := (p_page - 1) * p_size;
    v_total BIGINT;
BEGIN
    SELECT COUNT(*) INTO v_total
    FROM UserMaster u WHERE (p_center_id IS NULL OR u.CenterId = p_center_id)
        AND (p_keyword IS NULL OR u.UserName ILIKE '%'||p_keyword||'%'
        OR u.DisplayName ILIKE '%'||p_keyword||'%' OR u.Email ILIKE '%'||p_keyword||'%');

    RETURN QUERY SELECT u.UserId, u.UserName, u.DisplayName, u.Email, u.Phone,
        u.IsLocked, u.LastLoginAt, u.CreatedAt, v_total AS TotalCount
    FROM UserMaster u WHERE (p_center_id IS NULL OR u.CenterId = p_center_id)
        AND (p_keyword IS NULL OR u.UserName ILIKE '%'||p_keyword||'%'
        OR u.DisplayName ILIKE '%'||p_keyword||'%' OR u.Email ILIKE '%'||p_keyword||'%')
    ORDER BY u.CreatedAt DESC LIMIT p_size OFFSET v_offset;
END;
$$ LANGUAGE plpgsql;

-- 1b. RoleMaster
CREATE OR REPLACE FUNCTION fn_role_create(
    p_center_id BIGINT, p_name VARCHAR, p_desc VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO RoleMaster (CenterId, RoleName, Description, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_name, p_desc, NOW(), p_created_by) RETURNING RoleId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_role_get_all(p_center_id BIGINT DEFAULT NULL)
RETURNS TABLE(RoleId BIGINT, RoleName VARCHAR, Description VARCHAR, IsActive BOOLEAN, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT r.RoleId, r.RoleName, r.Description,
        TRUE::BOOLEAN AS IsActive, r.CreatedAt
    FROM RoleMaster r WHERE p_center_id IS NULL OR r.CenterId = p_center_id
    ORDER BY r.RoleName;
END;
$$ LANGUAGE plpgsql;

-- 1c. PermissionMaster
CREATE OR REPLACE FUNCTION fn_permission_get_all()
RETURNS TABLE(PermissionId BIGINT, PermissionCode VARCHAR, Description VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT p.PermissionId, p.PermissionCode, p.Description
    FROM PermissionMaster p ORDER BY p.PermissionCode;
END;
$$ LANGUAGE plpgsql;

-- 1d. RolePermissionMap
CREATE OR REPLACE FUNCTION fn_role_permission_assign(
    p_role_id BIGINT, p_permission_id BIGINT, p_center_id BIGINT, p_assigned_by BIGINT
) RETURNS VOID AS $$
BEGIN
    DELETE FROM RolePermissionMap
    WHERE RoleId = p_role_id AND PermissionId = p_permission_id AND CenterId = p_center_id;
    INSERT INTO RolePermissionMap (RoleId, PermissionId, CenterId, AssignedAt, AssignedBy)
    VALUES (p_role_id, p_permission_id, p_center_id, NOW(), p_assigned_by);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_role_permission_remove(
    p_role_id BIGINT, p_permission_id BIGINT, p_center_id BIGINT
) RETURNS VOID AS $$
BEGIN
    DELETE FROM RolePermissionMap
    WHERE RoleId = p_role_id AND PermissionId = p_permission_id AND CenterId = p_center_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_role_permission_get_by_role(p_role_id BIGINT, p_center_id BIGINT)
RETURNS TABLE(PermissionId BIGINT, PermissionCode VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT p.PermissionId, p.PermissionCode
    FROM RolePermissionMap rpm
    JOIN PermissionMaster p ON p.PermissionId = rpm.PermissionId
    WHERE rpm.RoleId = p_role_id AND rpm.CenterId = p_center_id;
END;
$$ LANGUAGE plpgsql;

-- 1e. UserRoleMap
CREATE OR REPLACE FUNCTION fn_user_role_assign(
    p_user_id BIGINT, p_role_id BIGINT, p_center_id BIGINT, p_assigned_by BIGINT
) RETURNS VOID AS $$
BEGIN
    DELETE FROM UserRoleMap WHERE UserId = p_user_id AND RoleId = p_role_id;
    INSERT INTO UserRoleMap (UserId, RoleId, CenterId, AssignedAt, AssignedBy)
    VALUES (p_user_id, p_role_id, p_center_id, NOW(), p_assigned_by);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_role_remove(p_user_id BIGINT, p_role_id BIGINT)
RETURNS VOID AS $$
BEGIN
    DELETE FROM UserRoleMap WHERE UserId = p_user_id AND RoleId = p_role_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_role_get_by_user(p_user_id BIGINT)
RETURNS TABLE(RoleId BIGINT, RoleName VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT r.RoleId, r.RoleName
    FROM UserRoleMap urm
    JOIN RoleMaster r ON r.RoleId = urm.RoleId
    WHERE urm.UserId = p_user_id;
END;
$$ LANGUAGE plpgsql;

-- 1f. LoginHistory
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

-- 1g. CenterUserMap
CREATE OR REPLACE FUNCTION fn_center_user_map(
    p_center_id BIGINT, p_user_id BIGINT, p_role_id BIGINT
) RETURNS VOID AS $$
BEGIN
    DELETE FROM CenterUserMap WHERE CenterId = p_center_id AND UserId = p_user_id;
    INSERT INTO CenterUserMap (CenterId, UserId, RoleId, AssignedAt)
    VALUES (p_center_id, p_user_id, p_role_id, NOW());
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_center_user_get_users(p_center_id BIGINT)
RETURNS TABLE(UserId BIGINT, UserName VARCHAR, DisplayName VARCHAR, RoleId BIGINT, AssignedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT u.UserId, u.UserName, u.DisplayName, cum.RoleId, cum.AssignedAt
    FROM CenterUserMap cum
    JOIN UserMaster u ON u.UserId = cum.UserId
    WHERE cum.CenterId = p_center_id;
END;
$$ LANGUAGE plpgsql;

-- 1h. UserSettings
CREATE OR REPLACE FUNCTION fn_user_setting_set(
    p_user_id BIGINT, p_key VARCHAR, p_value TEXT
) RETURNS VOID AS $$
BEGIN
    INSERT INTO UserSettings (UserId, SettingsKey, SettingsValue, UpdatedAt)
    VALUES (p_user_id, p_key, p_value, NOW())
    ON CONFLICT (UserId, SettingsKey)
    DO UPDATE SET SettingsValue = p_value, UpdatedAt = NOW();
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_user_setting_get(p_user_id BIGINT, p_key VARCHAR)
RETURNS TEXT AS $$
DECLARE v_val TEXT;
BEGIN
    SELECT SettingsValue INTO v_val FROM UserSettings
    WHERE UserId = p_user_id AND SettingsKey = p_key;
    RETURN v_val;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 2. Donor Management
-- ============================================================================

-- 2a. DonorMaster
CREATE OR REPLACE FUNCTION fn_donor_create(
    p_center_id BIGINT, p_code VARCHAR, p_first_name VARCHAR, p_last_name VARCHAR,
    p_gender VARCHAR, p_dob DATE, p_blood_group VARCHAR, p_phone VARCHAR,
    p_email VARCHAR, p_aadhaar VARCHAR, p_addr1 VARCHAR, p_addr2 VARCHAR,
    p_city VARCHAR, p_pincode VARCHAR, p_occupation VARCHAR,
    p_language VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DonorMaster (CenterId, DonorCode, FirstName, LastName, Gender,
        DateOfBirth, BloodGroup, Phone, Email, AadhaarNumber, AddressLine1,
        AddressLine2, City, Pincode, Occupation, PreferredLanguage, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_code, p_first_name, p_last_name, p_gender, p_dob,
        p_blood_group, p_phone, p_email, p_aadhaar, p_addr1, p_addr2, p_city,
        p_pincode, p_occupation, p_language, NOW(), p_created_by)
    RETURNING DonorId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_donor_get_by_id(p_donor_id BIGINT)
RETURNS TABLE(DonorId BIGINT, CenterId BIGINT, DonorCode VARCHAR, FirstName VARCHAR,
    LastName VARCHAR, Gender VARCHAR, DateOfBirth DATE, BloodGroup VARCHAR,
    Phone VARCHAR, Email VARCHAR, AadhaarNumber VARCHAR, AddressLine1 VARCHAR,
    AddressLine2 VARCHAR, City VARCHAR, Pincode VARCHAR, Occupation VARCHAR,
    PreferredLanguage VARCHAR, LastDonationDate DATE, TotalDonations INT,
    CreatedAt TIMESTAMPTZ, CreatedBy BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT * FROM DonorMaster WHERE DonorId = p_donor_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_donor_update(
    p_donor_id BIGINT, p_first_name VARCHAR, p_last_name VARCHAR,
    p_gender VARCHAR, p_dob DATE, p_blood_group VARCHAR, p_phone VARCHAR,
    p_email VARCHAR, p_aadhaar VARCHAR, p_addr1 VARCHAR, p_addr2 VARCHAR,
    p_city VARCHAR, p_pincode VARCHAR, p_occupation VARCHAR,
    p_language VARCHAR, p_updated_by BIGINT
) RETURNS VOID AS $$
BEGIN
    UPDATE DonorMaster
    SET FirstName = COALESCE(p_first_name, FirstName),
        LastName = COALESCE(p_last_name, LastName),
        Gender = COALESCE(p_gender, Gender),
        DateOfBirth = COALESCE(p_dob, DateOfBirth),
        BloodGroup = COALESCE(p_blood_group, BloodGroup),
        Phone = COALESCE(p_phone, Phone),
        Email = COALESCE(p_email, Email),
        AadhaarNumber = COALESCE(p_aadhaar, AadhaarNumber),
        AddressLine1 = COALESCE(p_addr1, AddressLine1),
        AddressLine2 = COALESCE(p_addr2, AddressLine2),
        City = COALESCE(p_city, City),
        Pincode = COALESCE(p_pincode, Pincode),
        Occupation = COALESCE(p_occupation, Occupation),
        PreferredLanguage = COALESCE(p_language, PreferredLanguage),
        UpdatedAt = NOW(),
        UpdatedBy = p_updated_by
    WHERE DonorId = p_donor_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_donor_search(
    p_center_id BIGINT, p_keyword VARCHAR DEFAULT NULL,
    p_blood_group VARCHAR DEFAULT NULL, p_gender VARCHAR DEFAULT NULL,
    p_page INT DEFAULT 1, p_size INT DEFAULT 20
) RETURNS TABLE(DonorId BIGINT, CenterId BIGINT, DonorCode VARCHAR, FirstName VARCHAR,
    LastName VARCHAR, Gender VARCHAR, Phone VARCHAR, BloodGroup VARCHAR,
    City VARCHAR, LastDonationDate DATE, TotalDonations INT, TotalCount BIGINT) AS $$
DECLARE v_offset INT := (p_page - 1) * p_size; v_total BIGINT;
BEGIN
    SELECT COUNT(*) INTO v_total FROM DonorMaster d
    WHERE (p_center_id IS NULL OR d.CenterId = p_center_id)
        AND (p_keyword IS NULL OR d.FirstName ILIKE '%'||p_keyword||'%'
            OR d.LastName ILIKE '%'||p_keyword||'%' OR d.Phone ILIKE '%'||p_keyword||'%'
            OR d.Email ILIKE '%'||p_keyword||'%' OR d.DonorCode ILIKE '%'||p_keyword||'%')
        AND (p_blood_group IS NULL OR d.BloodGroup = p_blood_group)
        AND (p_gender IS NULL OR d.Gender = p_gender);

    RETURN QUERY SELECT d.DonorId, d.CenterId, d.DonorCode, d.FirstName, d.LastName,
        d.Gender, d.Phone, d.BloodGroup, d.City, d.LastDonationDate, d.TotalDonations, v_total
    FROM DonorMaster d
    WHERE (p_center_id IS NULL OR d.CenterId = p_center_id)
        AND (p_keyword IS NULL OR d.FirstName ILIKE '%'||p_keyword||'%'
            OR d.LastName ILIKE '%'||p_keyword||'%' OR d.Phone ILIKE '%'||p_keyword||'%'
            OR d.Email ILIKE '%'||p_keyword||'%' OR d.DonorCode ILIKE '%'||p_keyword||'%')
        AND (p_blood_group IS NULL OR d.BloodGroup = p_blood_group)
        AND (p_gender IS NULL OR d.Gender = p_gender)
    ORDER BY d.CreatedAt DESC LIMIT p_size OFFSET v_offset;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_donor_get_by_phone(p_center_id BIGINT, p_phone VARCHAR)
RETURNS TABLE(DonorId BIGINT, FirstName VARCHAR, LastName VARCHAR, BloodGroup VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT d.DonorId, d.FirstName, d.LastName, d.BloodGroup
    FROM DonorMaster d WHERE d.CenterId = p_center_id AND d.Phone = p_phone;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_donor_update_donation_stats(p_donor_id BIGINT)
RETURNS VOID AS $$
BEGIN
    UPDATE DonorMaster
    SET TotalDonations = (SELECT COUNT(*) FROM DonorDonationHistory WHERE DonorId = p_donor_id),
        LastDonationDate = (SELECT MAX(DonationDate) FROM DonorDonationHistory WHERE DonorId = p_donor_id)
    WHERE DonorId = p_donor_id;
END;
$$ LANGUAGE plpgsql;

-- 2b. DonorHealthHistory
CREATE OR REPLACE FUNCTION fn_donor_health_create(
    p_center_id BIGINT, p_donor_id BIGINT, p_weight NUMERIC, p_temp NUMERIC,
    p_bp VARCHAR, p_hemoglobin NUMERIC, p_pulse INT, p_remarks VARCHAR, p_recorded_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DonorHealthHistory (CenterId, DonorId, VisitDate, WeightKg, Temperature,
        BloodPressure, Hemoglobin, PulseRate, Remarks, RecordedBy)
    VALUES (p_center_id, p_donor_id, NOW(), p_weight, p_temp, p_bp, p_hemoglobin,
        p_pulse, p_remarks, p_recorded_by)
    RETURNING DonorHealthHistoryId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_donor_health_get_by_donor(p_donor_id BIGINT)
RETURNS TABLE(DonorHealthHistoryId BIGINT, VisitDate TIMESTAMPTZ, WeightKg NUMERIC,
    Temperature NUMERIC, BloodPressure VARCHAR, Hemoglobin NUMERIC, PulseRate INT,
    Remarks VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT dhh.DonorHealthHistoryId, dhh.VisitDate, dhh.WeightKg,
        dhh.Temperature, dhh.BloodPressure, dhh.Hemoglobin, dhh.PulseRate, dhh.Remarks
    FROM DonorHealthHistory dhh
    WHERE dhh.DonorId = p_donor_id ORDER BY dhh.VisitDate DESC;
END;
$$ LANGUAGE plpgsql;

-- 2c. DonorDonationHistory
CREATE OR REPLACE FUNCTION fn_donor_donation_create(
    p_center_id BIGINT, p_donor_id BIGINT, p_collection_id BIGINT,
    p_donation_type VARCHAR, p_volume INT, p_bag_no VARCHAR, p_remarks VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DonorDonationHistory (CenterId, DonorId, CollectionId, DonationDate,
        DonationType, VolumeMl, BagNumber, Remarks, CreatedBy)
    VALUES (p_center_id, p_donor_id, p_collection_id, NOW(), p_donation_type,
        p_volume, p_bag_no, p_remarks, p_created_by)
    RETURNING DonationId INTO v_id;

    PERFORM fn_donor_update_donation_stats(p_donor_id);
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_donor_donation_get_by_donor(p_donor_id BIGINT)
RETURNS TABLE(DonationId BIGINT, DonationDate TIMESTAMPTZ, DonationType VARCHAR,
    VolumeMl INT, BagNumber VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT d.DonationId, d.DonationDate, d.DonationType, d.VolumeMl, d.BagNumber
    FROM DonorDonationHistory d WHERE d.DonorId = p_donor_id ORDER BY d.DonationDate DESC;
END;
$$ LANGUAGE plpgsql;

-- 2d. DeferralRecord
CREATE OR REPLACE FUNCTION fn_deferral_create(
    p_center_id BIGINT, p_donor_id BIGINT, p_reason VARCHAR,
    p_until DATE, p_notes VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DeferralRecord (CenterId, DonorId, DeferralDate, Reason, DeferralUntil, Notes, CreatedBy)
    VALUES (p_center_id, p_donor_id, NOW(), p_reason, p_until, p_notes, p_created_by)
    RETURNING DeferralId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_deferral_get_active(p_donor_id BIGINT)
RETURNS TABLE(DeferralId BIGINT, DeferralDate TIMESTAMPTZ, Reason VARCHAR, DeferralUntil DATE) AS $$
BEGIN
    RETURN QUERY SELECT d.DeferralId, d.DeferralDate, d.Reason, d.DeferralUntil
    FROM DeferralRecord d
    WHERE d.DonorId = p_donor_id
        AND (d.DeferralUntil IS NULL OR d.DeferralUntil >= CURRENT_DATE)
    ORDER BY d.DeferralDate DESC;
END;
$$ LANGUAGE plpgsql;

-- 2e. DonorAppointment
CREATE OR REPLACE FUNCTION fn_appointment_create(
    p_center_id BIGINT, p_donor_id BIGINT, p_date TIMESTAMPTZ,
    p_slot VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DonorAppointment (CenterId, DonorId, AppointmentDate, Slot, Status, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_donor_id, p_date, p_slot, 'Scheduled', NOW(), p_created_by)
    RETURNING AppointmentId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_appointment_update_status(p_id BIGINT, p_status VARCHAR)
RETURNS VOID AS $$
BEGIN
    UPDATE DonorAppointment SET Status = p_status WHERE AppointmentId = p_id;
END;
$$ LANGUAGE plpgsql;

-- 2f. DonorCommunicationLog
CREATE OR REPLACE FUNCTION fn_donor_comm_log(
    p_center_id BIGINT, p_donor_id BIGINT, p_channel VARCHAR,
    p_message VARCHAR, p_sent_by BIGINT, p_status VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DonorCommunicationLog (CenterId, DonorId, Channel, Message, SentAt, SentBy, Status)
    VALUES (p_center_id, p_donor_id, p_channel, p_message, NOW(), p_sent_by, p_status)
    RETURNING CommId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 3. Blood Camp & Collection Management
-- ============================================================================

-- 3a. BloodCampMaster
CREATE OR REPLACE FUNCTION fn_camp_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_organizer_id BIGINT,
    p_venue VARCHAR, p_city VARCHAR, p_date DATE, p_start TIMESTAMPTZ,
    p_end TIMESTAMPTZ, p_expected INT, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BloodCampMaster (CenterId, CampCode, CampName, OrganizerId, Venue,
        City, CampDate, StartTime, EndTime, TotalDonorsExpected, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_code, p_name, p_organizer_id, p_venue, p_city, p_date,
        p_start, p_end, p_expected, NOW(), p_created_by)
    RETURNING CampId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_get_by_id(p_camp_id BIGINT)
RETURNS TABLE(CampId BIGINT, CenterId BIGINT, CampCode VARCHAR, CampName VARCHAR,
    OrganizerId BIGINT, Venue VARCHAR, City VARCHAR, CampDate DATE,
    TotalDonorsExpected INT, TotalDonorsCollected INT, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT * FROM BloodCampMaster WHERE CampId = p_camp_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_get_upcoming(p_center_id BIGINT)
RETURNS TABLE(CampId BIGINT, CampName VARCHAR, Venue VARCHAR, City VARCHAR,
    CampDate DATE, TotalDonorsExpected INT) AS $$
BEGIN
    RETURN QUERY SELECT c.CampId, c.CampName, c.Venue, c.City, c.CampDate, c.TotalDonorsExpected
    FROM BloodCampMaster c
    WHERE c.CenterId = p_center_id AND c.CampDate >= CURRENT_DATE
    ORDER BY c.CampDate;
END;
$$ LANGUAGE plpgsql;

-- 3b. CampOrganizer
CREATE OR REPLACE FUNCTION fn_camp_organizer_create(
    p_center_id BIGINT, p_name VARCHAR, p_contact VARCHAR,
    p_phone VARCHAR, p_email VARCHAR, p_address VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CampOrganizer (CenterId, OrganizerName, ContactPerson, Phone, Email, Address, CreatedAt)
    VALUES (p_center_id, p_name, p_contact, p_phone, p_email, p_address, NOW())
    RETURNING OrganizerId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 3c. CampDonorMap
CREATE OR REPLACE FUNCTION fn_camp_register_donor(
    p_camp_id BIGINT, p_donor_id BIGINT, p_center_id BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CampDonorMap (CampId, DonorId, CenterId, RegisteredAt)
    VALUES (p_camp_id, p_donor_id, p_center_id, NOW())
    RETURNING CampDonorMapId INTO v_id;

    UPDATE BloodCampMaster
    SET TotalDonorsCollected = COALESCE(TotalDonorsCollected, 0) + 1
    WHERE CampId = p_camp_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 3d. CollectionRecord
CREATE OR REPLACE FUNCTION fn_collection_create(
    p_center_id BIGINT, p_branch_id BIGINT, p_camp_id BIGINT, p_donor_id BIGINT,
    p_bag_no VARCHAR, p_barcode VARCHAR, p_lot_no VARCHAR, p_volume INT,
    p_collector_id BIGINT, p_location_type VARCHAR, p_start TIMESTAMPTZ,
    p_end TIMESTAMPTZ, p_notes VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CollectionRecord (CenterId, BranchId, CampId, DonorId, BloodBagNumber,
        BagBarcode, BagLotNumber, BagVolumeMl, CollectorEmployeeId, CollectionLocationType,
        CollectionStartTime, CollectionEndTime, Notes, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_branch_id, p_camp_id, p_donor_id, p_bag_no, p_barcode,
        p_lot_no, p_volume, p_collector_id, p_location_type, p_start, p_end,
        p_notes, NOW(), p_created_by)
    RETURNING CollectionId INTO v_id;

    INSERT INTO BloodBagMaster (CenterId, BloodBagNumber, CollectionId, DonorId,
        BagBarcode, BagLotNumber, BagVolumeMl, BagStatus, InitialCollectedAt, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_bag_no, v_id, p_donor_id, p_barcode, p_lot_no,
        p_volume, 'Collected', COALESCE(p_start, NOW()), NOW(), p_created_by);

    PERFORM fn_donor_donation_create(p_center_id, p_donor_id, v_id, 'Voluntary',
        p_volume, p_bag_no, NULL, p_created_by);

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 3e. CollectionStaffMap
CREATE OR REPLACE FUNCTION fn_collection_assign_staff(
    p_collection_id BIGINT, p_employee_id BIGINT, p_role VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CollectionStaffMap (CollectionId, EmployeeId, Role, AssignedAt)
    VALUES (p_collection_id, p_employee_id, p_role, NOW())
    RETURNING CollectionStaffMapId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 4. Blood Testing & Screening
-- ============================================================================

-- 4a. BloodTestRecord
CREATE OR REPLACE FUNCTION fn_test_record_create(
    p_center_id BIGINT, p_collection_id BIGINT, p_bag_no VARCHAR,
    p_performed_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BloodTestRecord (CenterId, CollectionId, BagNumber, SampleTakenAt,
        PerformedBy, OverallStatus, CreatedAt)
    VALUES (p_center_id, p_collection_id, p_bag_no, NOW(), p_performed_by, 'Pending', NOW())
    RETURNING TestRecordId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_test_record_update_status(p_id BIGINT, p_status VARCHAR)
RETURNS VOID AS $$
BEGIN
    UPDATE BloodTestRecord SET OverallStatus = p_status WHERE TestRecordId = p_id;
END;
$$ LANGUAGE plpgsql;

-- 4b. BloodTestResult
CREATE OR REPLACE FUNCTION fn_test_result_add(
    p_center_id BIGINT, p_test_record_id BIGINT, p_bag_id BIGINT,
    p_test_code VARCHAR, p_result VARCHAR, p_method VARCHAR, p_kit_lot VARCHAR,
    p_performed_by BIGINT, p_remarks VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BloodTestResult (CenterId, TestRecordId, BagId, TestCode, Result,
        Method, KitLotNo, PerformedBy, PerformedAt, Remarks, CreatedAt)
    VALUES (p_center_id, p_test_record_id, p_bag_id, p_test_code, p_result,
        p_method, p_kit_lot, p_performed_by, NOW(), p_remarks, NOW())
    RETURNING TestResultId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_test_result_get_by_record(p_test_record_id BIGINT)
RETURNS TABLE(TestResultId BIGINT, TestCode VARCHAR, Result VARCHAR,
    Method VARCHAR, KitLotNo VARCHAR, PerformedBy BIGINT, PerformedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT tr.TestResultId, tr.TestCode, tr.Result, tr.Method,
        tr.KitLotNo, tr.PerformedBy, tr.PerformedAt
    FROM BloodTestResult tr WHERE tr.TestRecordId = p_test_record_id;
END;
$$ LANGUAGE plpgsql;

-- 4c. TestKitMaster
CREATE OR REPLACE FUNCTION fn_test_kit_create(
    p_center_id BIGINT, p_name VARCHAR, p_manufacturer VARCHAR,
    p_lot_no VARCHAR, p_expiry DATE
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO TestKitMaster (CenterId, KitName, Manufacturer, LotNumber, ExpiryDate, CreatedAt)
    VALUES (p_center_id, p_name, p_manufacturer, p_lot_no, p_expiry, NOW())
    RETURNING TestKitId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_test_kit_get_available(p_center_id BIGINT)
RETURNS TABLE(TestKitId BIGINT, KitName VARCHAR, LotNumber VARCHAR, ExpiryDate DATE) AS $$
BEGIN
    RETURN QUERY SELECT tk.TestKitId, tk.KitName, tk.LotNumber, tk.ExpiryDate
    FROM TestKitMaster tk
    WHERE tk.CenterId = p_center_id AND (tk.ExpiryDate IS NULL OR tk.ExpiryDate >= CURRENT_DATE);
END;
$$ LANGUAGE plpgsql;

-- 4d. QualityControlRecord
CREATE OR REPLACE FUNCTION fn_qc_create(
    p_center_id BIGINT, p_device_id BIGINT, p_detail VARCHAR, p_performed_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO QualityControlRecord (CenterId, DeviceId, QCDate, QCDetail, PerformedBy)
    VALUES (p_center_id, p_device_id, NOW(), p_detail, p_performed_by)
    RETURNING QCRecordId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 5. Component Preparation & Storage
-- ============================================================================

-- 5a. BloodBagMaster
CREATE OR REPLACE FUNCTION fn_bag_get_by_number(p_bag_no VARCHAR)
RETURNS TABLE(BagId BIGINT, CenterId BIGINT, BloodBagNumber VARCHAR, BagStatus VARCHAR,
    BagType VARCHAR, ExpiryDate DATE, DonorId BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT b.BagId, b.CenterId, b.BloodBagNumber, b.BagStatus,
        b.BagType, b.ExpiryDate, b.DonorId
    FROM BloodBagMaster b WHERE b.BloodBagNumber = p_bag_no;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_bag_update_status(p_bag_id BIGINT, p_status VARCHAR)
RETURNS VOID AS $$
BEGIN
    UPDATE BloodBagMaster SET BagStatus = p_status, UpdatedAt = NOW() WHERE BagId = p_bag_id;
END;
$$ LANGUAGE plpgsql;

-- 5b. ComponentPreparation
CREATE OR REPLACE FUNCTION fn_component_prepare(
    p_center_id BIGINT, p_bag_id BIGINT, p_component_type VARCHAR,
    p_volume INT, p_prepared_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_prep_id BIGINT; v_comp_id BIGINT;
BEGIN
    INSERT INTO ComponentPreparation (CenterId, ParentBagId, ComponentType, VolumeMl,
        PreparedBy, PreparedAt, CreatedAt)
    VALUES (p_center_id, p_bag_id, p_component_type, p_volume, p_prepared_by, NOW(), NOW())
    RETURNING PreparationId INTO v_prep_id;

    INSERT INTO ComponentMaster (CenterId, ComponentCode, ParentBagId, ComponentType,
        VolumeMl, CurrentStatus, CreatedAt)
    VALUES (p_center_id, 'CMP-' || v_prep_id, p_bag_id, p_component_type,
        p_volume, 'Available', NOW())
    RETURNING ComponentId INTO v_comp_id;

    INSERT INTO ComponentPreparationLog (CenterId, PreparationId, ComponentId, CreatedAt)
    VALUES (p_center_id, v_prep_id, v_comp_id, NOW());

    RETURN v_comp_id;
END;
$$ LANGUAGE plpgsql;

-- 5c. ComponentMaster
CREATE OR REPLACE FUNCTION fn_component_get_available(p_center_id BIGINT, p_blood_group VARCHAR DEFAULT NULL)
RETURNS TABLE(ComponentId BIGINT, ComponentCode VARCHAR, ComponentType VARCHAR,
    VolumeMl INT, ExpiryDate DATE, StorageLocation VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT c.ComponentId, c.ComponentCode, c.ComponentType, c.VolumeMl,
        c.ExpiryDate, c.StorageLocation
    FROM ComponentMaster c
    WHERE c.CenterId = p_center_id AND c.CurrentStatus = 'Available'
        AND (p_blood_group IS NULL OR EXISTS (
            SELECT 1 FROM BloodBagMaster b WHERE b.BagId = c.ParentBagId
            AND EXISTS (SELECT 1 FROM DonorMaster d WHERE d.DonorId = b.DonorId AND d.BloodGroup = p_blood_group)))
    ORDER BY c.ExpiryDate;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_component_update_status(p_component_id BIGINT, p_status VARCHAR)
RETURNS VOID AS $$
BEGIN
    UPDATE ComponentMaster SET CurrentStatus = p_status WHERE ComponentId = p_component_id;
END;
$$ LANGUAGE plpgsql;

-- 5d. ComponentStorage
CREATE OR REPLACE FUNCTION fn_component_store(
    p_center_id BIGINT, p_component_id BIGINT, p_fridge_id BIGINT,
    p_location VARCHAR, p_notes VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ComponentStorage (CenterId, ComponentId, FridgeId, StorageLocation, PlacedAt, Notes, CreatedAt)
    VALUES (p_center_id, p_component_id, p_fridge_id, p_location, NOW(), p_notes, NOW())
    RETURNING StorageId INTO v_id;

    UPDATE ComponentMaster SET StorageLocation = p_location WHERE ComponentId = p_component_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 5e. ComponentTransferLog
CREATE OR REPLACE FUNCTION fn_component_transfer(
    p_center_id BIGINT, p_component_id BIGINT, p_to_center_id BIGINT,
    p_transport_details VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ComponentTransferLog (CenterId, ComponentId, FromCenterId, ToCenterId,
        TransferDate, TransportDetails, CreatedBy)
    VALUES (p_center_id, p_component_id, p_center_id, p_to_center_id, NOW(),
        p_transport_details, p_created_by)
    RETURNING TransferId INTO v_id;

    UPDATE ComponentMaster
    SET CurrentStatus = 'Transferred', CenterId = p_to_center_id
    WHERE ComponentId = p_component_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 5f. DiscardRecord
CREATE OR REPLACE FUNCTION fn_component_discard(
    p_center_id BIGINT, p_bag_id BIGINT, p_component_id BIGINT,
    p_reason VARCHAR, p_discarded_by BIGINT, p_notes VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DiscardRecord (CenterId, BagId, ComponentId, DiscardReason,
        DiscardedAt, DiscardedBy, Notes)
    VALUES (p_center_id, p_bag_id, p_component_id, p_reason, NOW(), p_discarded_by, p_notes)
    RETURNING DiscardId INTO v_id;

    IF p_component_id IS NOT NULL THEN
        UPDATE ComponentMaster SET CurrentStatus = 'Discarded' WHERE ComponentId = p_component_id;
    END IF;

    IF p_bag_id IS NOT NULL THEN
        UPDATE BloodBagMaster SET BagStatus = 'Discarded', UpdatedAt = NOW() WHERE BagId = p_bag_id;
    END IF;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 6. Inventory Management
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_inventory_upsert(
    p_center_id BIGINT, p_component_type VARCHAR, p_blood_group VARCHAR,
    p_available INT DEFAULT 0, p_reserved INT DEFAULT 0, p_quarantined INT DEFAULT 0,
    p_updated_by BIGINT DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO InventoryStock (CenterId, ComponentType, BloodGroup, AvailableQty,
        ReservedQty, QuarantinedQty, LastUpdatedAt, LastUpdatedBy, CreatedAt)
    VALUES (p_center_id, p_component_type, p_blood_group, p_available, p_reserved,
        p_quarantined, NOW(), p_updated_by, NOW())
    ON CONFLICT (CenterId, ComponentType, BloodGroup)
    DO UPDATE SET AvailableQty = InventoryStock.AvailableQty + p_available,
        ReservedQty = InventoryStock.ReservedQty + p_reserved,
        QuarantinedQty = InventoryStock.QuarantinedQty + p_quarantined,
        LastUpdatedAt = NOW(), LastUpdatedBy = p_updated_by
    RETURNING InventoryStockId INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inventory_get_stock(p_center_id BIGINT)
RETURNS TABLE(ComponentType VARCHAR, BloodGroup VARCHAR, AvailableQty INT,
    ReservedQty INT, QuarantinedQty INT) AS $$
BEGIN
    RETURN QUERY SELECT s.ComponentType, s.BloodGroup, s.AvailableQty,
        s.ReservedQty, s.QuarantinedQty
    FROM InventoryStock s
    WHERE s.CenterId = p_center_id AND s.AvailableQty > 0
    ORDER BY s.BloodGroup, s.ComponentType;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inventory_get_summary(p_center_id BIGINT)
RETURNS TABLE(BloodGroup VARCHAR, TotalAvailable INT, TotalReserved INT) AS $$
BEGIN
    RETURN QUERY SELECT s.BloodGroup, SUM(s.AvailableQty)::INT, SUM(s.ReservedQty)::INT
    FROM InventoryStock s WHERE s.CenterId = p_center_id
    GROUP BY s.BloodGroup ORDER BY s.BloodGroup;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_inventory_transaction_log(
    p_center_id BIGINT, p_tx_type VARCHAR, p_ref_type VARCHAR, p_ref_id VARCHAR,
    p_component_id BIGINT, p_bag_id BIGINT, p_qty INT, p_from VARCHAR,
    p_to VARCHAR, p_notes VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO InventoryTransactionLog (CenterId, TransactionType, ReferenceType,
        ReferenceId, ComponentId, BagId, Quantity, FromLocation, ToLocation,
        Notes, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_tx_type, p_ref_type, p_ref_id, p_component_id, p_bag_id,
        p_qty, p_from, p_to, p_notes, NOW(), p_created_by)
    RETURNING InventoryTxId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 7. Hospital, Issue & Requests
-- ============================================================================

-- 7a. HospitalMaster
CREATE OR REPLACE FUNCTION fn_hospital_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_address VARCHAR,
    p_contact VARCHAR, p_phone VARCHAR, p_email VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO HospitalMaster (CenterId, HospitalCode, HospitalName, Address,
        ContactPerson, Phone, Email, CreatedAt)
    VALUES (p_center_id, p_code, p_name, p_address, p_contact, p_phone, p_email, NOW())
    RETURNING HospitalId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 7b. PatientRequest
CREATE OR REPLACE FUNCTION fn_patient_request_create(
    p_center_id BIGINT, p_hospital_id BIGINT, p_patient_name VARCHAR,
    p_age INT, p_gender VARCHAR, p_blood_group VARCHAR, p_component_type VARCHAR,
    p_units INT, p_urgency VARCHAR, p_requested_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO PatientRequest (CenterId, HospitalId, PatientName, PatientAge,
        PatientGender, BloodGroup, ComponentType, UnitsRequested, RequestDate,
        RequestUrgency, RequestedByUserId)
    VALUES (p_center_id, p_hospital_id, p_patient_name, p_age, p_gender, p_blood_group,
        p_component_type, p_units, NOW(), p_urgency, p_requested_by)
    RETURNING RequestId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_patient_request_get_pending(p_center_id BIGINT)
RETURNS TABLE(RequestId BIGINT, PatientName VARCHAR, BloodGroup VARCHAR,
    ComponentType VARCHAR, UnitsRequested INT, RequestUrgency VARCHAR,
    RequestDate TIMESTAMPTZ, HospitalName VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT pr.RequestId, pr.PatientName, pr.BloodGroup, pr.ComponentType,
        pr.UnitsRequested, pr.RequestUrgency, pr.RequestDate, h.HospitalName
    FROM PatientRequest pr
    LEFT JOIN HospitalMaster h ON h.HospitalId = pr.HospitalId
    WHERE pr.CenterId = p_center_id
        AND pr.RelatedIssueId IS NULL
    ORDER BY pr.RequestUrgency = 'Emergency' DESC, pr.RequestDate;
END;
$$ LANGUAGE plpgsql;

-- 7c. CrossMatchRecord
CREATE OR REPLACE FUNCTION fn_crossmatch_create(
    p_center_id BIGINT, p_request_id BIGINT, p_component_id BIGINT,
    p_result VARCHAR, p_method VARCHAR, p_performed_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO CrossMatchRecord (CenterId, RequestId, ComponentId, Result,
        Method, PerformedBy, PerformedAt, CreatedAt)
    VALUES (p_center_id, p_request_id, p_component_id, p_result, p_method,
        p_performed_by, NOW(), NOW())
    RETURNING CrossMatchId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 7d. IssueRecord
CREATE OR REPLACE FUNCTION fn_issue_create(
    p_center_id BIGINT, p_component_id BIGINT, p_bag_id BIGINT,
    p_patient_name VARCHAR, p_hospital_id BIGINT, p_issued_by BIGINT,
    p_issue_type VARCHAR, p_slip_no VARCHAR, p_notes VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO IssueRecord (CenterId, ComponentId, BagId, PatientName, HospitalId,
        IssueDate, IssuedByUserId, IssueType, IssueSlipNumber, Notes)
    VALUES (p_center_id, p_component_id, p_bag_id, p_patient_name, p_hospital_id,
        NOW(), p_issued_by, p_issue_type, p_slip_no, p_notes)
    RETURNING IssueRecordId INTO v_id;

    IF p_component_id IS NOT NULL THEN
        UPDATE ComponentMaster SET CurrentStatus = 'Issued' WHERE ComponentId = p_component_id;
    END IF;

    IF p_bag_id IS NOT NULL THEN
        UPDATE BloodBagMaster SET BagStatus = 'Issued', UpdatedAt = NOW() WHERE BagId = p_bag_id;
    END IF;

    PERFORM fn_inventory_upsert(p_center_id,
        (SELECT ComponentType FROM ComponentMaster WHERE ComponentId = p_component_id),
        NULL, -1, 0, 0, p_issued_by);

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 7e. ReturnRecord
CREATE OR REPLACE FUNCTION fn_return_create(
    p_center_id BIGINT, p_issue_id BIGINT, p_component_id BIGINT,
    p_reason VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ReturnRecord (CenterId, IssueRecordId, ComponentId, ReturnDate, Reason, CreatedBy)
    VALUES (p_center_id, p_issue_id, p_component_id, NOW(), p_reason, p_created_by)
    RETURNING ReturnId INTO v_id;

    IF p_component_id IS NOT NULL THEN
        UPDATE ComponentMaster SET CurrentStatus = 'Returned' WHERE ComponentId = p_component_id;
    END IF;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 7f. ReplacementDonor
CREATE OR REPLACE FUNCTION fn_replacement_donor_register(
    p_center_id BIGINT, p_request_id BIGINT, p_donor_id BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ReplacementDonor (CenterId, PatientRequestId, DonorId, DonatedAt)
    VALUES (p_center_id, p_request_id, p_donor_id, NOW())
    RETURNING ReplacementDonorId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 7g. RequestStatusLog
CREATE OR REPLACE FUNCTION fn_request_status_log(
    p_request_id BIGINT, p_old_status VARCHAR, p_new_status VARCHAR,
    p_changed_by BIGINT, p_notes VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO RequestStatusLog (RequestId, OldStatus, NewStatus, ChangedAt, ChangedBy, Notes)
    VALUES (p_request_id, p_old_status, p_new_status, NOW(), p_changed_by, p_notes)
    RETURNING RequestStatusLogId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 8. Billing & Finance
-- ============================================================================

-- 8a. ServiceChargeMaster
CREATE OR REPLACE FUNCTION fn_service_charge_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR,
    p_amount NUMERIC, p_active BOOLEAN DEFAULT TRUE
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ServiceChargeMaster (CenterId, ServiceCode, ServiceName, Amount, IsActive, CreatedAt)
    VALUES (p_center_id, p_code, p_name, p_amount, p_active, NOW())
    RETURNING ServiceChargeId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 8b. BillingTransaction
CREATE OR REPLACE FUNCTION fn_billing_create(
    p_center_id BIGINT, p_invoice_no VARCHAR, p_patient_id BIGINT,
    p_total NUMERIC, p_tax NUMERIC DEFAULT 0, p_discount NUMERIC DEFAULT 0,
    p_payment_status VARCHAR DEFAULT 'Pending', p_payment_mode VARCHAR DEFAULT NULL,
    p_created_by BIGINT DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BillingTransaction (CenterId, InvoiceNumber, PatientId, TotalAmount,
        TaxAmount, Discount, PaymentStatus, PaymentMode, InvoiceDate, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_invoice_no, p_patient_id, p_total, p_tax, p_discount,
        p_payment_status, p_payment_mode, NOW(), NOW(), p_created_by)
    RETURNING BillingTransactionId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 8c. InvoiceDetail
CREATE OR REPLACE FUNCTION fn_invoice_detail_add(
    p_billing_id BIGINT, p_component_id BIGINT, p_service_charge_id BIGINT,
    p_service_name VARCHAR, p_qty INT, p_unit_price NUMERIC
) RETURNS BIGINT AS $$
DECLARE v_line_total NUMERIC; v_id BIGINT;
BEGIN
    v_line_total := COALESCE(p_qty, 1) * COALESCE(p_unit_price, 0);
    INSERT INTO InvoiceDetail (BillingTransactionId, ComponentId, ServiceChargeId,
        ServiceName, Quantity, UnitPrice, LineTotal, CreatedAt)
    VALUES (p_billing_id, p_component_id, p_service_charge_id, p_service_name,
        p_qty, p_unit_price, v_line_total, NOW())
    RETURNING InvoiceDetailId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 8d. PaymentRecord
CREATE OR REPLACE FUNCTION fn_payment_create(
    p_billing_id BIGINT, p_center_id BIGINT, p_amount NUMERIC,
    p_mode VARCHAR, p_reference VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO PaymentRecord (BillingTransactionId, CenterId, PaymentDate, Amount,
        PaymentMode, Reference, CreatedBy)
    VALUES (p_billing_id, p_center_id, NOW(), p_amount, p_mode, p_reference, p_created_by)
    RETURNING PaymentId INTO v_id;

    UPDATE BillingTransaction
    SET PaymentStatus = CASE WHEN (SELECT COALESCE(SUM(Amount),0) FROM PaymentRecord
        WHERE BillingTransactionId = p_billing_id) >= TotalAmount THEN 'Paid' ELSE 'Partial' END,
        PaymentMode = COALESCE(p_mode, PaymentMode)
    WHERE BillingTransactionId = p_billing_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- 8e. ExpenseMaster
CREATE OR REPLACE FUNCTION fn_expense_create(
    p_center_id BIGINT, p_category VARCHAR, p_amount NUMERIC,
    p_notes VARCHAR, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ExpenseMaster (CenterId, ExpenseDate, Category, Amount, Notes, CreatedBy)
    VALUES (p_center_id, NOW(), p_category, p_amount, p_notes, p_created_by)
    RETURNING ExpenseId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 9. Reporting
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_report_monthly_generate(
    p_center_id BIGINT, p_year INT, p_month INT
) RETURNS BIGINT AS $$
DECLARE
    v_id BIGINT;
    v_data TEXT;
BEGIN
    v_data := jsonb_build_object(
        'center_id', p_center_id,
        'year', p_year,
        'month', p_month,
        'generated_at', NOW(),
        'donors_new', (SELECT COUNT(*) FROM DonorMaster d WHERE d.CenterId = p_center_id
            AND EXTRACT(YEAR FROM d.CreatedAt) = p_year AND EXTRACT(MONTH FROM d.CreatedAt) = p_month),
        'collections', (SELECT COUNT(*) FROM CollectionRecord c WHERE c.CenterId = p_center_id
            AND EXTRACT(YEAR FROM c.CreatedAt) = p_year AND EXTRACT(MONTH FROM c.CreatedAt) = p_month),
        'units_issued', (SELECT COUNT(*) FROM IssueRecord i WHERE i.CenterId = p_center_id
            AND EXTRACT(YEAR FROM i.IssueDate) = p_year AND EXTRACT(MONTH FROM i.IssueDate) = p_month),
        'discards', (SELECT COUNT(*) FROM DiscardRecord d WHERE d.CenterId = p_center_id
            AND EXTRACT(YEAR FROM d.DiscardedAt) = p_year AND EXTRACT(MONTH FROM d.DiscardedAt) = p_month)
    )::TEXT;

    INSERT INTO MonthlyReportLog (CenterId, ReportYear, ReportMonth, DataSnapshot, CreatedAt)
    VALUES (p_center_id, p_year, p_month, v_data, NOW())
    RETURNING MonthlyReportId INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 10. Communication & Notifications
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_notification_create(
    p_center_id BIGINT, p_type VARCHAR, p_title VARCHAR, p_body VARCHAR,
    p_audience VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO NotificationMaster (CenterId, NotificationType, Title, Body,
        TargetAudience, IsActive, CreatedAt)
    VALUES (p_center_id, p_type, p_title, p_body, p_audience, TRUE, NOW())
    RETURNING NotificationId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_outbox_send(
    p_center_id BIGINT, p_channel VARCHAR, p_recipient VARCHAR,
    p_message TEXT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO OutboxLog (CenterId, Channel, Recipient, Message, SentAt, Status, CreatedAt)
    VALUES (p_center_id, p_channel, p_recipient, p_message, NOW(), 'Sent', NOW())
    RETURNING OutboxId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 11. Emergency
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_emergency_request_create(
    p_center_id BIGINT, p_hospital_id BIGINT, p_patient_name VARCHAR,
    p_blood_group VARCHAR, p_component_type VARCHAR, p_units INT,
    p_requested_by BIGINT, p_notes VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO EmergencyRequest (CenterId, HospitalId, PatientName, BloodGroup,
        ComponentType, UnitsRequired, RequestStatus, RequestedAt, RequestedByUserId, Notes)
    VALUES (p_center_id, p_hospital_id, p_patient_name, p_blood_group, p_component_type,
        p_units, 'Open', NOW(), p_requested_by, p_notes)
    RETURNING EmergencyRequestId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_emergency_donor_response(
    p_emergency_id BIGINT, p_donor_id BIGINT, p_contact VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO EmergencyDonorResponse (EmergencyRequestId, DonorId, ResponseContact,
        RespondedAt, IsVerified)
    VALUES (p_emergency_id, p_donor_id, p_contact, NOW(), FALSE)
    RETURNING ResponseId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 12. System Administration
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_backup_log(
    p_center_id BIGINT, p_type VARCHAR, p_path VARCHAR, p_status VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BackupLog (CenterId, BackupType, BackupPath, BackupStartedAt, Status, CreatedAt)
    VALUES (p_center_id, p_type, p_path, NOW(), p_status, NOW())
    RETURNING BackupLogId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_config_get(p_center_id BIGINT, p_key VARCHAR)
RETURNS TEXT AS $$
DECLARE v_val TEXT;
BEGIN
    SELECT ConfigValue INTO v_val FROM SystemConfig
    WHERE CenterId = p_center_id AND ConfigKey = p_key;
    RETURN v_val;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_config_set(
    p_center_id BIGINT, p_key VARCHAR, p_value TEXT, p_desc VARCHAR DEFAULT NULL
) RETURNS VOID AS $$
BEGIN
    INSERT INTO SystemConfig (CenterId, ConfigKey, ConfigValue, Description, CreatedAt)
    VALUES (p_center_id, p_key, p_value, p_desc, NOW())
    ON CONFLICT (CenterId, ConfigKey)
    DO UPDATE SET ConfigValue = p_value, Description = COALESCE(p_desc, SystemConfig.Description);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_center_config_set(
    p_center_id BIGINT, p_key VARCHAR, p_value TEXT
) RETURNS VOID AS $$
BEGIN
    INSERT INTO CenterConfig (CenterId, ConfigKey, ConfigValue, CreatedAt)
    VALUES (p_center_id, p_key, p_value, NOW())
    ON CONFLICT (CenterId, ConfigKey)
    DO UPDATE SET ConfigValue = p_value;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 13. Master Data — Generic CRUD
-- ============================================================================

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

CREATE OR REPLACE FUNCTION fn_lookup_get_by_type(p_type_code VARCHAR)
RETURNS TABLE(ValueCode VARCHAR, ValueText VARCHAR, SortOrder INT) AS $$
BEGIN
    RETURN QUERY SELECT lv.ValueCode, lv.ValueText, lv.SortOrder
    FROM LookupValue lv JOIN LookupType lt ON lt.LookupTypeId = lv.LookupTypeId
    WHERE lt.TypeCode = p_type_code AND lv.IsActive = TRUE ORDER BY lv.SortOrder;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 14. API Integration
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_api_integration_create(
    p_center_id BIGINT, p_name VARCHAR, p_url VARCHAR,
    p_api_key VARCHAR, p_username VARCHAR, p_password VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ApiIntegrationMaster (CenterId, IntegrationName, BaseUrl, ApiKey,
        Username, PasswordEncrypted, CreatedAt)
    VALUES (p_center_id, p_name, p_url, p_api_key, p_username, p_password, NOW())
    RETURNING ApiIntegrationId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_api_response_log(
    p_center_id BIGINT, p_integration_id BIGINT, p_request TEXT,
    p_response TEXT, p_status_code VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO ApiResponseLog (CenterId, ApiIntegrationId, RequestPayload,
        ResponsePayload, StatusCode, CalledAt)
    VALUES (p_center_id, p_integration_id, p_request, p_response, p_status_code, NOW())
    RETURNING ApiResponseLogId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- End of Stored Procedures — 89 tables, ~120 functions
-- ============================================================================
