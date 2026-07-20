-- ============================================================================
-- Stored Procedures: BranchMaster, DepartmentMaster, DesignationMaster, EmployeeMaster
-- ============================================================================

DROP FUNCTION IF EXISTS fn_branch_get_by_id(BIGINT);
DROP FUNCTION IF EXISTS fn_branch_get_by_center(BIGINT);
DROP FUNCTION IF EXISTS fn_department_get_by_id(BIGINT);
DROP FUNCTION IF EXISTS fn_department_get_by_center(BIGINT);
DROP FUNCTION IF EXISTS fn_designation_get_by_id(BIGINT);
DROP FUNCTION IF EXISTS fn_designation_get_by_center(BIGINT);
DROP FUNCTION IF EXISTS fn_employee_get_by_id(BIGINT);
DROP FUNCTION IF EXISTS fn_employee_get_by_center(BIGINT);

-- ── BranchMaster ──
CREATE OR REPLACE FUNCTION fn_branch_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR,
    p_address_line1 VARCHAR, p_address_line2 VARCHAR, p_city VARCHAR,
    p_state VARCHAR, p_pincode VARCHAR, p_phone VARCHAR, p_email VARCHAR,
    p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO BranchMaster (CenterId, BranchCode, BranchName, AddressLine1,
        AddressLine2, City, State, Pincode, Phone, Email, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_code, p_name, p_address_line1, p_address_line2,
        p_city, p_state, p_pincode, p_phone, p_email, NOW(), p_created_by)
    RETURNING BranchId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_branch_update(
    p_branch_id BIGINT, p_code VARCHAR, p_name VARCHAR,
    p_address_line1 VARCHAR, p_address_line2 VARCHAR, p_city VARCHAR,
    p_state VARCHAR, p_pincode VARCHAR, p_phone VARCHAR, p_email VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE BranchMaster SET
        BranchCode = COALESCE(p_code, BranchCode),
        BranchName = COALESCE(p_name, BranchName),
        AddressLine1 = COALESCE(p_address_line1, AddressLine1),
        AddressLine2 = COALESCE(p_address_line2, AddressLine2),
        City = COALESCE(p_city, City),
        State = COALESCE(p_state, State),
        Pincode = COALESCE(p_pincode, Pincode),
        Phone = COALESCE(p_phone, Phone),
        Email = COALESCE(p_email, Email)
    WHERE BranchId = p_branch_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_branch_get_by_id(p_branch_id BIGINT)
RETURNS TABLE(BranchId BIGINT, CenterId BIGINT, BranchCode VARCHAR, BranchName VARCHAR,
    AddressLine1 VARCHAR, AddressLine2 VARCHAR, City VARCHAR, State VARCHAR,
    Pincode VARCHAR, Phone VARCHAR, Email VARCHAR, CreatedAt TIMESTAMPTZ, CreatedBy BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT b.BranchId, b.CenterId, b.BranchCode, b.BranchName,
        b.AddressLine1, b.AddressLine2, b.City, b.State, b.Pincode, b.Phone, b.Email,
        b.CreatedAt, b.CreatedBy
    FROM BranchMaster b WHERE b.BranchId = p_branch_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_branch_get_by_center(p_center_id BIGINT)
RETURNS TABLE(BranchId BIGINT, CenterId BIGINT, BranchCode VARCHAR, BranchName VARCHAR,
    AddressLine1 VARCHAR, AddressLine2 VARCHAR, City VARCHAR, State VARCHAR,
    Pincode VARCHAR, Phone VARCHAR, Email VARCHAR, CreatedAt TIMESTAMPTZ, CreatedBy BIGINT) AS $$
BEGIN
    RETURN QUERY SELECT b.BranchId, b.CenterId, b.BranchCode, b.BranchName,
        b.AddressLine1, b.AddressLine2, b.City, b.State, b.Pincode, b.Phone, b.Email,
        b.CreatedAt, b.CreatedBy
    FROM BranchMaster b WHERE b.CenterId = p_center_id ORDER BY b.BranchName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_branch_delete(p_branch_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM BranchMaster WHERE BranchId = p_branch_id;
END;
$$ LANGUAGE plpgsql;

-- ── DepartmentMaster ──
CREATE OR REPLACE FUNCTION fn_department_create(
    p_center_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_description VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DepartmentMaster (CenterId, DepartmentCode, DepartmentName, Description, CreatedAt)
    VALUES (p_center_id, p_code, p_name, p_description, NOW())
    RETURNING DepartmentId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_department_update(
    p_department_id BIGINT, p_code VARCHAR, p_name VARCHAR, p_description VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE DepartmentMaster SET
        DepartmentCode = COALESCE(p_code, DepartmentCode),
        DepartmentName = COALESCE(p_name, DepartmentName),
        Description = COALESCE(p_description, Description)
    WHERE DepartmentId = p_department_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_department_get_by_id(p_department_id BIGINT)
RETURNS TABLE(DepartmentId BIGINT, CenterId BIGINT, DepartmentCode VARCHAR,
    DepartmentName VARCHAR, Description VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT d.DepartmentId, d.CenterId, d.DepartmentCode,
        d.DepartmentName, d.Description, d.CreatedAt
    FROM DepartmentMaster d WHERE d.DepartmentId = p_department_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_department_get_by_center(p_center_id BIGINT)
RETURNS TABLE(DepartmentId BIGINT, CenterId BIGINT, DepartmentCode VARCHAR,
    DepartmentName VARCHAR, Description VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT d.DepartmentId, d.CenterId, d.DepartmentCode,
        d.DepartmentName, d.Description, d.CreatedAt
    FROM DepartmentMaster d WHERE d.CenterId = p_center_id ORDER BY d.DepartmentName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_department_delete(p_department_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM DepartmentMaster WHERE DepartmentId = p_department_id;
END;
$$ LANGUAGE plpgsql;

-- ── DesignationMaster ──
CREATE OR REPLACE FUNCTION fn_designation_create(
    p_center_id BIGINT, p_name VARCHAR
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO DesignationMaster (CenterId, DesignationName, CreatedAt)
    VALUES (p_center_id, p_name, NOW())
    RETURNING DesignationId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_designation_update(
    p_designation_id BIGINT, p_name VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE DesignationMaster SET DesignationName = COALESCE(p_name, DesignationName)
    WHERE DesignationId = p_designation_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_designation_get_by_id(p_designation_id BIGINT)
RETURNS TABLE(DesignationId BIGINT, CenterId BIGINT, DesignationName VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT d.DesignationId, d.CenterId, d.DesignationName, d.CreatedAt
    FROM DesignationMaster d WHERE d.DesignationId = p_designation_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_designation_get_by_center(p_center_id BIGINT)
RETURNS TABLE(DesignationId BIGINT, CenterId BIGINT, DesignationName VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT d.DesignationId, d.CenterId, d.DesignationName, d.CreatedAt
    FROM DesignationMaster d WHERE d.CenterId = p_center_id ORDER BY d.DesignationName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_designation_delete(p_designation_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM DesignationMaster WHERE DesignationId = p_designation_id;
END;
$$ LANGUAGE plpgsql;

-- ── EmployeeMaster ──
DROP FUNCTION IF EXISTS fn_employee_create(BIGINT, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, BIGINT, DATE, BIGINT);
CREATE OR REPLACE FUNCTION fn_employee_create(
    p_center_id BIGINT, p_code VARCHAR, p_first_name VARCHAR, p_last_name VARCHAR,
    p_email VARCHAR, p_phone VARCHAR, p_designation VARCHAR, p_department_id BIGINT,
    p_join_date TIMESTAMP, p_created_by BIGINT
) RETURNS BIGINT AS $$
DECLARE v_id BIGINT;
BEGIN
    INSERT INTO EmployeeMaster (CenterId, EmployeeCode, FirstName, LastName, Email,
        Phone, Designation, DepartmentId, JoinDate, IsActive, CreatedAt, CreatedBy)
    VALUES (p_center_id, p_code, p_first_name, p_last_name, p_email, p_phone,
        p_designation, p_department_id, p_join_date::DATE, TRUE, NOW(), p_created_by)
    RETURNING EmployeeId INTO v_id;
    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS fn_employee_update(BIGINT, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, BIGINT, DATE);
CREATE OR REPLACE FUNCTION fn_employee_update(
    p_employee_id BIGINT, p_code VARCHAR, p_first_name VARCHAR, p_last_name VARCHAR,
    p_email VARCHAR, p_phone VARCHAR, p_designation VARCHAR, p_department_id BIGINT,
    p_join_date TIMESTAMP
) RETURNS VOID AS $$
BEGIN
    UPDATE EmployeeMaster SET
        EmployeeCode = COALESCE(p_code, EmployeeCode),
        FirstName = COALESCE(p_first_name, FirstName),
        LastName = COALESCE(p_last_name, LastName),
        Email = COALESCE(p_email, Email),
        Phone = COALESCE(p_phone, Phone),
        Designation = COALESCE(p_designation, Designation),
        DepartmentId = COALESCE(p_department_id, DepartmentId),
        JoinDate = COALESCE(p_join_date, JoinDate),
        UpdatedAt = NOW()
    WHERE EmployeeId = p_employee_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_employee_get_by_id(p_employee_id BIGINT)
RETURNS TABLE(EmployeeId BIGINT, CenterId BIGINT, EmployeeCode VARCHAR,
    FirstName VARCHAR, LastName VARCHAR, Email VARCHAR, Phone VARCHAR,
    Designation VARCHAR, DepartmentId BIGINT, JoinDate DATE, IsActive BOOLEAN,
    CreatedAt TIMESTAMPTZ, CreatedBy BIGINT, UpdatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT e.EmployeeId, e.CenterId, e.EmployeeCode, e.FirstName,
        e.LastName, e.Email, e.Phone, e.Designation, e.DepartmentId, e.JoinDate,
        e.IsActive, e.CreatedAt, e.CreatedBy, e.UpdatedAt
    FROM EmployeeMaster e WHERE e.EmployeeId = p_employee_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_employee_get_by_center(p_center_id BIGINT)
RETURNS TABLE(EmployeeId BIGINT, CenterId BIGINT, EmployeeCode VARCHAR,
    FirstName VARCHAR, LastName VARCHAR, Email VARCHAR, Phone VARCHAR,
    Designation VARCHAR, DepartmentId BIGINT, JoinDate DATE, IsActive BOOLEAN,
    CreatedAt TIMESTAMPTZ, CreatedBy BIGINT, UpdatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT e.EmployeeId, e.CenterId, e.EmployeeCode, e.FirstName,
        e.LastName, e.Email, e.Phone, e.Designation, e.DepartmentId, e.JoinDate,
        e.IsActive, e.CreatedAt, e.CreatedBy, e.UpdatedAt
    FROM EmployeeMaster e WHERE e.CenterId = p_center_id ORDER BY e.FirstName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_employee_toggle_active(p_employee_id BIGINT) RETURNS VOID AS $$
BEGIN
    UPDATE EmployeeMaster SET IsActive = NOT IsActive, UpdatedAt = NOW()
    WHERE EmployeeId = p_employee_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_employee_delete(p_employee_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM EmployeeMaster WHERE EmployeeId = p_employee_id;
END;
$$ LANGUAGE plpgsql;
