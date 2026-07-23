-- ============================================================================
-- Stored Procedures: CampOrganizer Master CRUD
-- ============================================================================

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
