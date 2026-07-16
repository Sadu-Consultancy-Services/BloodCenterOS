-- Seed: Admin user for local dev
-- Password: admin@123 (BCrypt hashed)

UPDATE usermaster SET
  passwordhash = '$2a$11$EDuMXQ8JdPSZXOahB17Ln.rSagzAC54LhwFOhKQW1bguBo.DmzZ5i',
  passwordsalt = ''
WHERE userid = 1;
