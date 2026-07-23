-- ============================================================================
-- BloodCenterOS — Patch 20260724-001: CampOrganizer Master CRUD
-- Description: Stored procedures + seed data for CampOrganizer table
-- Apply: psql -U postgres -d bloodcenter -f patch_20260724_001_camp_organizer.sql
-- ============================================================================

-- 1. Stored Procedures -------------------------------------------------------

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

CREATE OR REPLACE FUNCTION fn_camp_organizer_get_by_id(p_organizer_id BIGINT)
RETURNS TABLE(OrganizerId BIGINT, CenterId BIGINT, OrganizerName VARCHAR,
    ContactPerson VARCHAR, Phone VARCHAR, Email VARCHAR, Address VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT o.OrganizerId, o.CenterId, o.OrganizerName,
        o.ContactPerson, o.Phone, o.Email, o.Address, o.CreatedAt
    FROM CampOrganizer o WHERE o.OrganizerId = p_organizer_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_organizer_get_by_center(p_center_id BIGINT)
RETURNS TABLE(OrganizerId BIGINT, CenterId BIGINT, OrganizerName VARCHAR,
    ContactPerson VARCHAR, Phone VARCHAR, Email VARCHAR, Address VARCHAR, CreatedAt TIMESTAMPTZ) AS $$
BEGIN
    RETURN QUERY SELECT o.OrganizerId, o.CenterId, o.OrganizerName,
        o.ContactPerson, o.Phone, o.Email, o.Address, o.CreatedAt
    FROM CampOrganizer o WHERE o.CenterId = p_center_id ORDER BY o.OrganizerName;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_organizer_update(
    p_organizer_id BIGINT, p_name VARCHAR, p_contact VARCHAR,
    p_phone VARCHAR, p_email VARCHAR, p_address VARCHAR
) RETURNS VOID AS $$
BEGIN
    UPDATE CampOrganizer SET
        OrganizerName = COALESCE(p_name, OrganizerName),
        ContactPerson = COALESCE(p_contact, ContactPerson),
        Phone = COALESCE(p_phone, Phone),
        Email = COALESCE(p_email, Email),
        Address = COALESCE(p_address, Address)
    WHERE OrganizerId = p_organizer_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_camp_organizer_delete(p_organizer_id BIGINT) RETURNS VOID AS $$
BEGIN
    DELETE FROM CampOrganizer WHERE OrganizerId = p_organizer_id;
END;
$$ LANGUAGE plpgsql;

-- 2. Seed Data ---------------------------------------------------------------

INSERT INTO CampOrganizer (CenterId, OrganizerName, CreatedAt)
SELECT 1, name, NOW() FROM (VALUES
    ('Rotary Club'), ('Lions Club'), ('Corporate Partner'),
    ('Educational Institution'), ('NGO'), ('Religious Organization'),
    ('Government'), ('Other')
) AS t(name)
WHERE NOT EXISTS (SELECT 1 FROM CampOrganizer WHERE OrganizerName = t.name);
