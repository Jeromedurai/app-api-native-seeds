-- =============================================
-- USERS TABLE INSERT SCRIPT
-- Sample data for testing and development
-- =============================================

-- First, ensure the required tables exist and have the correct structure
-- This script assumes the database schema has been created using the CREATE_DATABASE_SCHEMA script

-- =============================================
-- INSERT SAMPLE USERS
-- =============================================

-- Clear existing data (optional - uncomment if you want to start fresh)
-- DELETE FROM UserRoles;
-- DELETE FROM Users;
-- DBCC CHECKIDENT ('Users', RESEED, 1);

-- =============================================
-- ADMIN USER (TenantId = 1, RoleId = 1)
-- =============================================
INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    Phone,
    PasswordHash,
    Salt,
    TenantId,
    Active,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt,
    UpdatedAt,
    ProfilePicture,
    DateOfBirth,
    Gender,
    Timezone,
    Language,
    Country,
    City,
    State,
    PostalCode,
    Bio,
    Website,
    CompanyName,
    JobTitle,
    PreferredContactMethod,
    NotificationSettings,
    PrivacySettings
) VALUES (
    'Admin',
    'User',
    'admin@example.com',
    '+1-555-0100',
    -- Password: Admin123! (SHA256 hash with salt)
    CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'Admin123!' + 'admin_salt_001'), 2),
    'admin_salt_001',
    1, -- TenantId
    1, -- Active
    1, -- EmailVerified
    1, -- PhoneVerified
    0, -- LoginAttempts
    0, -- AccountLocked
    GETUTCDATE(), -- CreatedAt
    GETUTCDATE(), -- UpdatedAt
    'https://example.com/images/admin-avatar.jpg',
    '1985-01-15',
    'Male',
    'America/New_York',
    'en',
    'United States',
    'New York',
    'NY',
    '10001',
    'System Administrator with full access to all features.',
    'https://admin.example.com',
    'Example Corp',
    'System Administrator',
    'Email',
    '{"email": true, "sms": true, "push": true}',
    '{"profile_public": false, "contact_public": false}'
);

-- =============================================
-- REGULAR USER (TenantId = 1, RoleId = 2)
-- =============================================
INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    Phone,
    PasswordHash,
    Salt,
    TenantId,
    Active,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt,
    UpdatedAt,
    ProfilePicture,
    DateOfBirth,
    Gender,
    Timezone,
    Language,
    Country,
    City,
    State,
    PostalCode,
    Bio,
    Website,
    CompanyName,
    JobTitle,
    PreferredContactMethod,
    NotificationSettings,
    PrivacySettings
) VALUES (
    'John',
    'Doe',
    'john.doe@example.com',
    '+1-555-0101',
    -- Password: User123! (SHA256 hash with salt)
    CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'User123!' + 'user_salt_001'), 2),
    'user_salt_001',
    1, -- TenantId
    1, -- Active
    1, -- EmailVerified
    0, -- PhoneVerified
    0, -- LoginAttempts
    0, -- AccountLocked
    GETUTCDATE(), -- CreatedAt
    GETUTCDATE(), -- UpdatedAt
    'https://example.com/images/john-avatar.jpg',
    '1990-05-20',
    'Male',
    'America/Chicago',
    'en',
    'United States',
    'Chicago',
    'IL',
    '60601',
    'Software developer passionate about creating user-friendly applications.',
    'https://johndoe.dev',
    'Tech Solutions Inc',
    'Senior Developer',
    'Email',
    '{"email": true, "sms": false, "push": true}',
    '{"profile_public": true, "contact_public": false}'
);

-- =============================================
-- ANOTHER REGULAR USER (TenantId = 1, RoleId = 2)
-- =============================================
INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    Phone,
    PasswordHash,
    Salt,
    TenantId,
    Active,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt,
    UpdatedAt,
    ProfilePicture,
    DateOfBirth,
    Gender,
    Timezone,
    Language,
    Country,
    City,
    State,
    PostalCode,
    Bio,
    Website,
    CompanyName,
    JobTitle,
    PreferredContactMethod,
    NotificationSettings,
    PrivacySettings
) VALUES (
    'Jane',
    'Smith',
    'jane.smith@example.com',
    '+1-555-0102',
    -- Password: User123! (SHA256 hash with salt)
    CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'User123!' + 'user_salt_002'), 2),
    'user_salt_002',
    1, -- TenantId
    1, -- Active
    1, -- EmailVerified
    1, -- PhoneVerified
    0, -- LoginAttempts
    0, -- AccountLocked
    GETUTCDATE(), -- CreatedAt
    GETUTCDATE(), -- UpdatedAt
    'https://example.com/images/jane-avatar.jpg',
    '1988-12-10',
    'Female',
    'America/Los_Angeles',
    'en',
    'United States',
    'Los Angeles',
    'CA',
    '90210',
    'UX Designer focused on creating intuitive user experiences.',
    'https://janesmith.design',
    'Creative Design Studio',
    'UX Designer',
    'Phone',
    '{"email": true, "sms": true, "push": false}',
    '{"profile_public": true, "contact_public": true}'
);

-- =============================================
-- TEST USER FOR LOGIN TESTING (TenantId = 1, RoleId = 2)
-- =============================================
INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    Phone,
    PasswordHash,
    Salt,
    TenantId,
    Active,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt,
    UpdatedAt,
    ProfilePicture,
    DateOfBirth,
    Gender,
    Timezone,
    Language,
    Country,
    City,
    State,
    PostalCode,
    Bio,
    Website,
    CompanyName,
    JobTitle,
    PreferredContactMethod,
    NotificationSettings,
    PrivacySettings
) VALUES (
    'Test',
    'User',
    'test@example.com',
    '+1-555-0103',
    -- Password: Test123! (SHA256 hash with salt)
    CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'Test123!' + 'test_salt_001'), 2),
    'test_salt_001',
    1, -- TenantId
    1, -- Active
    1, -- EmailVerified
    1, -- PhoneVerified
    0, -- LoginAttempts
    0, -- AccountLocked
    GETUTCDATE(), -- CreatedAt
    GETUTCDATE(), -- UpdatedAt
    NULL, -- ProfilePicture
    '1995-08-25',
    'Other',
    'UTC',
    'en',
    'United States',
    'Test City',
    'TS',
    '12345',
    'Test user account for development and testing purposes.',
    NULL, -- Website
    'Test Company',
    'Test Engineer',
    'Email',
    '{"email": true, "sms": false, "push": false}',
    '{"profile_public": false, "contact_public": false}'
);

-- =============================================
-- INACTIVE USER FOR TESTING (TenantId = 1, RoleId = 2)
-- =============================================
INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    Phone,
    PasswordHash,
    Salt,
    TenantId,
    Active,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt,
    UpdatedAt,
    ProfilePicture,
    DateOfBirth,
    Gender,
    Timezone,
    Language,
    Country,
    City,
    State,
    PostalCode,
    Bio,
    Website,
    CompanyName,
    JobTitle,
    PreferredContactMethod,
    NotificationSettings,
    PrivacySettings
) VALUES (
    'Inactive',
    'User',
    'inactive@example.com',
    '+1-555-0104',
    -- Password: Inactive123! (SHA256 hash with salt)
    CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'Inactive123!' + 'inactive_salt_001'), 2),
    'inactive_salt_001',
    1, -- TenantId
    0, -- Active (INACTIVE)
    1, -- EmailVerified
    0, -- PhoneVerified
    0, -- LoginAttempts
    0, -- AccountLocked
    GETUTCDATE(), -- CreatedAt
    GETUTCDATE(), -- UpdatedAt
    NULL, -- ProfilePicture
    '1992-03-15',
    'Male',
    'UTC',
    'en',
    'United States',
    'Inactive City',
    'IC',
    '54321',
    'Inactive user account for testing inactive user scenarios.',
    NULL, -- Website
    'Inactive Corp',
    'Former Employee',
    'Email',
    '{"email": false, "sms": false, "push": false}',
    '{"profile_public": false, "contact_public": false}'
);

-- =============================================
-- LOCKED USER FOR TESTING (TenantId = 1, RoleId = 2)
-- =============================================
INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    Phone,
    PasswordHash,
    Salt,
    TenantId,
    Active,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt,
    UpdatedAt,
    ProfilePicture,
    DateOfBirth,
    Gender,
    Timezone,
    Language,
    Country,
    City,
    State,
    PostalCode,
    Bio,
    Website,
    CompanyName,
    JobTitle,
    PreferredContactMethod,
    NotificationSettings,
    PrivacySettings
) VALUES (
    'Locked',
    'User',
    'locked@example.com',
    '+1-555-0105',
    -- Password: Locked123! (SHA256 hash with salt)
    CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'Locked123!' + 'locked_salt_001'), 2),
    'locked_salt_001',
    1, -- TenantId
    1, -- Active
    1, -- EmailVerified
    0, -- PhoneVerified
    5, -- LoginAttempts (MAX ATTEMPTS)
    1, -- AccountLocked (LOCKED)
    GETUTCDATE(), -- CreatedAt
    GETUTCDATE(), -- UpdatedAt
    NULL, -- ProfilePicture
    '1987-11-30',
    'Female',
    'UTC',
    'en',
    'United States',
    'Locked City',
    'LC',
    '67890',
    'Locked user account for testing account lockout scenarios.',
    NULL, -- Website
    'Locked Corp',
    'Security Test',
    'Email',
    '{"email": false, "sms": false, "push": false}',
    '{"profile_public": false, "contact_public": false}'
);

-- =============================================
-- MULTI-TENANT USER (TenantId = 2, RoleId = 2)
-- =============================================
INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    Phone,
    PasswordHash,
    Salt,
    TenantId,
    Active,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt,
    UpdatedAt,
    ProfilePicture,
    DateOfBirth,
    Gender,
    Timezone,
    Language,
    Country,
    City,
    State,
    PostalCode,
    Bio,
    Website,
    CompanyName,
    JobTitle,
    PreferredContactMethod,
    NotificationSettings,
    PrivacySettings
) VALUES (
    'Multi',
    'Tenant',
    'multi.tenant@example2.com',
    '+1-555-0201',
    -- Password: Multi123! (SHA256 hash with salt)
    CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'Multi123!' + 'multi_salt_001'), 2),
    'multi_salt_001',
    2, -- TenantId (DIFFERENT TENANT)
    1, -- Active
    1, -- EmailVerified
    0, -- PhoneVerified
    0, -- LoginAttempts
    0, -- AccountLocked
    GETUTCDATE(), -- CreatedAt
    GETUTCDATE(), -- UpdatedAt
    NULL, -- ProfilePicture
    '1993-07-08',
    'Male',
    'Europe/London',
    'en',
    'United Kingdom',
    'London',
    'England',
    'SW1A 1AA',
    'Multi-tenant user for testing tenant isolation.',
    'https://multitenant.example2.com',
    'Multi Corp',
    'Tenant Manager',
    'Email',
    '{"email": true, "sms": true, "push": true}',
    '{"profile_public": true, "contact_public": true}'
);

-- =============================================
-- VERIFY INSERTED DATA
-- =============================================
SELECT 
    UserId,
    FirstName,
    LastName,
    Email,
    Phone,
    Active,
    TenantId,
    EmailVerified,
    PhoneVerified,
    LoginAttempts,
    AccountLocked,
    CreatedAt
FROM Users 
ORDER BY UserId;

-- =============================================
-- SAMPLE LOGIN CREDENTIALS FOR TESTING
-- =============================================
/*
ADMIN USER:
- Email: admin@example.com
- Password: Admin123!

REGULAR USERS:
- Email: john.doe@example.com
- Password: User123!

- Email: jane.smith@example.com
- Password: User123!

TEST USER:
- Email: test@example.com
- Password: Test123!

INACTIVE USER:
- Email: inactive@example.com
- Password: Inactive123!
- Status: Inactive (will fail login)

LOCKED USER:
- Email: locked@example.com
- Password: Locked123!
- Status: Account Locked (will fail login)

MULTI-TENANT USER:
- Email: multi.tenant@example2.com
- Password: Multi123!
- TenantId: 2
*/

-- =============================================
-- NOTES:
-- =============================================
/*
1. All passwords are hashed using SHA256 with unique salts
2. The stored procedure SP_USER_LOGIN expects hashed passwords
3. When testing login, use the plain text passwords shown above
4. The stored procedure will hash the input password with the stored salt
5. Make sure the Roles table has at least RoleId 1 (Admin) and RoleId 2 (User)
6. Make sure the UserRoles table has the appropriate role assignments
7. All users are created with TenantId = 1 except the multi-tenant user (TenantId = 2)
8. The inactive and locked users are useful for testing edge cases
*/
