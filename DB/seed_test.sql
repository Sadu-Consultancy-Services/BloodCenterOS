-- Seed: Integration test base data

INSERT INTO BloodCenterMaster (CenterId, CenterCode, CenterName, LicenseNumber, AddressLine1, City, District, State, Pincode, Phone, Email, Website, IsActive, CreatedAt, CreatedBy)
VALUES (1, 'MUM01', 'Main Blood Center', 'LIC-001', '123 Test Street', 'Mumbai', 'Mumbai City', 'Maharashtra', '400001', '022-12345678', 'admin@bloodcenter.os', 'https://bloodcenter.os', TRUE, NOW(), 1)
ON CONFLICT (CenterId) DO UPDATE SET CenterName = EXCLUDED.CenterName;

INSERT INTO RoleMaster (RoleId, CenterId, RoleName, Description, CreatedAt)
VALUES (1, 1, 'Administrator', 'System Administrator', NOW())
ON CONFLICT (RoleId) DO NOTHING;

INSERT INTO UserMaster (UserId, CenterId, UserName, PasswordHash, PasswordSalt, DisplayName, Email, Phone, IsLocked, CreatedAt)
VALUES (1, 1, 'admin', '$2a$11$EDuMXQ8JdPSZXOahB17Ln.rSagzAC54LhwFOhKQW1bguBo.DmzZ5i', '', 'Administrator', 'admin@bloodcenter.os', '9876543210', FALSE, NOW())
ON CONFLICT (UserId) DO UPDATE SET PasswordHash = EXCLUDED.PasswordHash;

INSERT INTO UserRoleMap (UserId, RoleId, CenterId, AssignedAt)
SELECT 1, 1, 1, NOW()
WHERE NOT EXISTS (SELECT 1 FROM UserRoleMap WHERE UserId = 1 AND RoleId = 1 AND CenterId = 1);

INSERT INTO BloodGroupMaster (BloodGroupId, BloodGroupCode, Description)
VALUES
  (1, 'A+',  'A Positive'),
  (2, 'A-',  'A Negative'),
  (3, 'B+',  'B Positive'),
  (4, 'B-',  'B Negative'),
  (5, 'AB+', 'AB Positive'),
  (6, 'AB-', 'AB Negative'),
  (7, 'O+',  'O Positive'),
  (8, 'O-',  'O Negative')
ON CONFLICT (BloodGroupId) DO NOTHING;
