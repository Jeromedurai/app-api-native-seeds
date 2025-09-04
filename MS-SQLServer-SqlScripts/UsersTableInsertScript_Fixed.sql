-- =============================================
-- USERS TABLE INSERT SCRIPT - FIXED VERSION
-- Sample data for testing and development
-- Handles HASHBYTES compatibility issues
-- =============================================

-- First, ensure the required tables exist and have the correct structure
-- This script assumes the database schema has been created using the CREATE_DATABASE_SCHEMA script

-- =============================================
-- CHECK SQL SERVER VERSION AND HASHBYTES SUPPORT
-- =============================================
DECLARE @SQLServerVersion NVARCHAR(128)
DECLARE @HashAlgorithm NVARCHAR(10)

SELECT @SQLServerVersion = @@VERSION

-- Determine which hash algorithm to use based on SQL Server version
IF @SQLServerVersion LIKE '%SQL Server 2012%' OR @SQLServerVersion LIKE '%SQL Server 2014%' 
   OR @SQLServerVersion LIKE '%SQL Server 2016%' OR @SQLServerVersion LIKE '%SQL Server 2017%'
   OR @SQLServerVersion LIKE '%SQL Server 2019%' OR @SQLServerVersion LIKE '%SQL Server 2022%'
BEGIN
    SET @HashAlgorithm = 'SHA256'
    PRINT 'Using SHA256 hash algorithm (SQL Server 2012+)'
END
ELSE
BEGIN
    SET @HashAlgorithm = 'SHA1'
    PRINT 'Using SHA1 hash algorithm (SQL Server 2008 or earlier)'
END

-- Test HASHBYTES function to ensure it works
DECLARE @TestHash VARBINARY(32)
DECLARE @TestString NVARCHAR(100) = 'Admin123!admin_salt_001'

IF @HashAlgorithm = 'SHA256'
BEGIN
    SET @TestHash = HASHBYTES('SHA256', @TestString)
    IF @TestHash IS NULL
    BEGIN
        PRINT 'WARNING: SHA256 returned NULL, falling back to SHA1'
        SET @HashAlgorithm = 'SHA1'
        SET @TestHash = HASHBYTES('SHA1', @TestString)
    END
END
ELSE
BEGIN
    SET @TestHash = HASHBYTES('SHA1', @TestString)
END

IF @TestHash IS NULL
BEGIN
    RAISERROR('HASHBYTES function is not working properly. Please check SQL Server configuration.', 16, 1)
    RETURN
END

PRINT 'HASHBYTES test successful. Hash length: ' + CAST(LEN(@TestHash) AS NVARCHAR(10))

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
    -- Password: Admin123! (Hash with salt using appropriate algorithm)
    CASE 
        WHEN @HashAlgorithm = 'SHA256' THEN CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'Admin123!' + 'admin_salt_001'), 2)
        ELSE CONVERT(NVARCHAR(255), HASHBYTES('SHA1', 'Admin123!' + 'admin_salt_001'), 2)
    END,
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
    -- Password: User123! (Hash with salt using appropriate algorithm)
    CASE 
        WHEN @HashAlgorithm = 'SHA256' THEN CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'User123!' + 'user_salt_001'), 2)
        ELSE CONVERT(NVARCHAR(255), HASHBYTES('SHA1', 'User123!' + 'user_salt_001'), 2)
    END,
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
    -- Password: Test123! (Hash with salt using appropriate algorithm)
    CASE 
        WHEN @HashAlgorithm = 'SHA256' THEN CONVERT(NVARCHAR(255), HASHBYTES('SHA256', 'Test123!' + 'test_salt_001'), 2)
        ELSE CONVERT(NVARCHAR(255), HASHBYTES('SHA1', 'Test123!' + 'test_salt_001'), 2)
    END,
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
    CreatedAt,
    CASE 
        WHEN PasswordHash IS NULL THEN 'NULL - HASHING FAILED'
        ELSE 'OK - ' + CAST(LEN(PasswordHash) AS NVARCHAR(10)) + ' chars'
    END as PasswordHashStatus
FROM Users 
ORDER BY UserId;

-- =============================================
-- TEST HASHING FUNCTION
-- =============================================
PRINT 'Testing password hashing...'

DECLARE @TestPassword NVARCHAR(50) = 'Test123!'
DECLARE @TestSalt NVARCHAR(50) = 'test_salt_001'
DECLARE @TestHashResult NVARCHAR(255)

-- Test the same hashing logic used in the INSERT
SET @TestHashResult = CASE 
    WHEN @HashAlgorithm = 'SHA256' THEN CONVERT(NVARCHAR(255), HASHBYTES('SHA256', @TestPassword + @TestSalt), 2)
    ELSE CONVERT(NVARCHAR(255), HASHBYTES('SHA1', @TestPassword + @TestSalt), 2)
END

IF @TestHashResult IS NULL
BEGIN
    PRINT 'ERROR: Password hashing is still returning NULL!'
    PRINT 'SQL Server Version: ' + @SQLServerVersion
    PRINT 'Hash Algorithm: ' + @HashAlgorithm
    
    -- Try alternative approaches
    PRINT 'Trying alternative hashing approaches...'
    
    -- Approach 1: Explicit casting
    SET @TestHashResult = CONVERT(NVARCHAR(255), HASHBYTES('SHA1', CAST(@TestPassword + @TestSalt AS NVARCHAR(MAX))), 2)
    PRINT 'Alternative 1 (Explicit casting): ' + ISNULL(@TestHashResult, 'NULL')
    
    -- Approach 2: CONCAT function
    SET @TestHashResult = CONVERT(NVARCHAR(255), HASHBYTES('SHA1', CONCAT(@TestPassword, @TestSalt)), 2)
    PRINT 'Alternative 2 (CONCAT): ' + ISNULL(@TestHashResult, 'NULL')
    
    -- Approach 3: Simple string
    SET @TestHashResult = CONVERT(NVARCHAR(255), HASHBYTES('SHA1', 'simple_test'), 2)
    PRINT 'Alternative 3 (Simple string): ' + ISNULL(@TestHashResult, 'NULL')
END
ELSE
BEGIN
    PRINT 'SUCCESS: Password hashing is working correctly!'
    PRINT 'Hash Algorithm: ' + @HashAlgorithm
    PRINT 'Hash Length: ' + CAST(LEN(@TestHashResult) AS NVARCHAR(10))
END

-- =============================================
-- SAMPLE LOGIN CREDENTIALS FOR TESTING
-- =============================================
/*
ADMIN USER:
- Email: admin@example.com
- Password: Admin123!

REGULAR USER:
- Email: john.doe@example.com
- Password: User123!

TEST USER:
- Email: test@example.com
- Password: Test123!
*/

-- =============================================
-- TROUBLESHOOTING NOTES:
-- =============================================
/*
If HASHBYTES is still returning NULL, try these solutions:

1. CHECK SQL SERVER VERSION:
   SELECT @@VERSION

2. CHECK HASHBYTES SUPPORT:
   SELECT HASHBYTES('SHA1', 'test') as SHA1Test
   SELECT HASHBYTES('SHA256', 'test') as SHA256Test

3. ALTERNATIVE HASHING METHODS:
   - Use SHA1 instead of SHA256 for older SQL Server versions
   - Use MD5 as last resort (not recommended for production)
   - Implement custom hashing in application code

4. COMMON ISSUES:
   - SQL Server version too old
   - String encoding problems
   - NULL input values
   - Insufficient permissions

5. WORKAROUND:
   If all else fails, you can hash passwords in your C# code and store the hash directly
*/
