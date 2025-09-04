-- =============================================
-- ROLES AND USER-ROLES INSERT SCRIPT
-- Required for the user authentication system to work
-- =============================================

-- First, ensure the required tables exist and have the correct structure
-- This script assumes the database schema has been created using the CREATE_DATABASE_SCHEMA script

-- =============================================
-- INSERT REQUIRED ROLES
-- =============================================

-- Clear existing data (optional - uncomment if you want to start fresh)
-- DELETE FROM UserRoles;
-- DELETE FROM Roles;
-- DBCC CHECKIDENT ('Roles', RESEED, 1);

-- Insert the required roles
INSERT INTO Roles (
    RoleName,
    RoleDescription,
    Active,
    CreatedAt,
    UpdatedAt
) VALUES 
    ('Admin', 'System Administrator with full access to all features', 1, GETUTCDATE(), GETUTCDATE()),
    ('User', 'Regular user with standard access permissions', 1, GETUTCDATE(), GETUTCDATE()),
    ('Manager', 'Team manager with elevated permissions', 1, GETUTCDATE(), GETUTCDATE()),
    ('Guest', 'Limited access user for temporary access', 1, GETUTCDATE(), GETUTCDATE()),
    ('Support', 'Customer support representative', 1, GETUTCDATE(), GETUTCDATE());

-- =============================================
-- VERIFY INSERTED ROLES
-- =============================================
SELECT 
    RoleId,
    RoleName,
    RoleDescription,
    Active,
    CreatedAt
FROM Roles 
ORDER BY RoleId;

-- =============================================
-- INSERT USER-ROLE ASSIGNMENTS
-- =============================================

-- Note: This assumes the Users table has been populated with the UsersTableInsertScript.sql
-- Make sure to run that script first, or adjust the UserId values accordingly

-- Admin user gets Admin role (RoleId = 1)
INSERT INTO UserRoles (
    UserId,
    RoleId,
    CreatedAt
) VALUES (
    (SELECT UserId FROM Users WHERE Email = 'admin@example.com'),
    1, -- Admin role
    GETUTCDATE()
);

-- Regular users get User role (RoleId = 2)
INSERT INTO UserRoles (
    UserId,
    RoleId,
    CreatedAt
) VALUES 
    ((SELECT UserId FROM Users WHERE Email = 'john.doe@example.com'), 2, GETUTCDATE()),
    ((SELECT UserId FROM Users WHERE Email = 'jane.smith@example.com'), 2, GETUTCDATE()),
    ((SELECT UserId FROM Users WHERE Email = 'test@example.com'), 2, GETUTCDATE()),
    ((SELECT UserId FROM Users WHERE Email = 'inactive@example.com'), 2, GETUTCDATE()),
    ((SELECT UserId FROM Users WHERE Email = 'locked@example.com'), 2, GETUTCDATE());

-- Multi-tenant user gets User role (RoleId = 2)
INSERT INTO UserRoles (
    UserId,
    RoleId,
    CreatedAt
) VALUES (
    (SELECT UserId FROM Users WHERE Email = 'multi.tenant@example2.com'),
    2, -- User role
    GETUTCDATE()
);

-- Optional: Give some users multiple roles for testing
-- John Doe gets both User and Manager roles
INSERT INTO UserRoles (
    UserId,
    RoleId,
    CreatedAt
) VALUES (
    (SELECT UserId FROM Users WHERE Email = 'john.doe@example.com'),
    3, -- Manager role
    GETUTCDATE()
);

-- =============================================
-- VERIFY USER-ROLE ASSIGNMENTS
-- =============================================
SELECT 
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    r.RoleId,
    r.RoleName,
    r.RoleDescription,
    ur.CreatedAt as RoleAssignedAt
FROM Users u
INNER JOIN UserRoles ur ON u.UserId = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.RoleId
ORDER BY u.UserId, r.RoleId;

-- =============================================
-- COMPLETE USER DATA VERIFICATION
-- =============================================
-- This query shows all users with their roles and key information
SELECT 
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    u.Phone,
    u.Active,
    u.TenantId,
    u.EmailVerified,
    u.PhoneVerified,
    u.LoginAttempts,
    u.AccountLocked,
    STRING_AGG(r.RoleName, ', ') as Roles,
    u.CreatedAt
FROM Users u
LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.RoleId
GROUP BY 
    u.UserId, u.FirstName, u.LastName, u.Email, u.Phone,
    u.Active, u.TenantId, u.EmailVerified, u.PhoneVerified,
    u.LoginAttempts, u.AccountLocked, u.CreatedAt
ORDER BY u.UserId;

-- =============================================
-- TEST QUERIES FOR STORED PROCEDURES
-- =============================================

-- Test query to verify the SP_USER_LOGIN stored procedure will work
-- This simulates what the stored procedure does internally
SELECT 
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    u.Phone,
    u.Active,
    u.TenantId,
    u.LastLogin,
    ur.RoleId,
    r.RoleName,
    r.RoleDescription,
    0 AS RememberMe -- This would come from the stored procedure parameter
FROM Users u
LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.RoleId
WHERE (u.Email = 'test@example.com' OR u.Phone = 'test@example.com')
    AND u.Active = 1;

-- =============================================
-- SAMPLE LOGIN CREDENTIALS FOR TESTING
-- =============================================
/*
ADMIN USER:
- Email: admin@example.com
- Password: Admin123!
- Role: Admin (RoleId = 1)

REGULAR USERS:
- Email: john.doe@example.com
- Password: User123!
- Roles: User (RoleId = 2), Manager (RoleId = 3)

- Email: jane.smith@example.com
- Password: User123!
- Role: User (RoleId = 2)

TEST USER:
- Email: test@example.com
- Password: Test123!
- Role: User (RoleId = 2)

INACTIVE USER:
- Email: inactive@example.com
- Password: Inactive123!
- Role: User (RoleId = 2)
- Status: Inactive (will fail login)

LOCKED USER:
- Email: locked@example.com
- Password: Locked123!
- Role: User (RoleId = 2)
- Status: Account Locked (will fail login)

MULTI-TENANT USER:
- Email: multi.tenant@example2.com
- Password: Multi123!
- Role: User (RoleId = 2)
- TenantId: 2
*/

-- =============================================
-- NOTES:
-- =============================================
/*
1. Run the UsersTableInsertScript.sql FIRST before running this script
2. This script creates the required roles and assigns them to users
3. The stored procedures expect these roles to exist
4. RoleId 1 = Admin, RoleId 2 = User (these are hardcoded in some stored procedures)
5. Users can have multiple roles (like John Doe who is both User and Manager)
6. The verification queries help ensure everything is set up correctly
7. Use the sample login credentials to test the authentication system
8. Make sure the database schema has been created before running these scripts
*/
