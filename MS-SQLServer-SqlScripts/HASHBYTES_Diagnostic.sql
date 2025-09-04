-- =============================================
-- HASHBYTES DIAGNOSTIC SCRIPT
-- Use this to troubleshoot HASHBYTES returning NULL
-- =============================================

PRINT '=== HASHBYTES DIAGNOSTIC START ==='

-- =============================================
-- 1. CHECK SQL SERVER VERSION
-- =============================================
PRINT ''
PRINT '1. SQL SERVER VERSION:'
SELECT @@VERSION as SQLServerVersion

-- =============================================
-- 2. CHECK HASHBYTES FUNCTION SUPPORT
-- =============================================
PRINT ''
PRINT '2. TESTING HASHBYTES FUNCTION SUPPORT:'

-- Test basic HASHBYTES functionality
DECLARE @TestString NVARCHAR(100) = 'test_string_123'
DECLARE @HashResult VARBINARY(32)

-- Test SHA1 (supported in SQL Server 2005+)
PRINT 'Testing SHA1...'
SET @HashResult = HASHBYTES('SHA1', @TestString)
IF @HashResult IS NULL
    PRINT '  SHA1: FAILED (returned NULL)'
ELSE
    PRINT '  SHA1: SUCCESS - Hash length: ' + CAST(LEN(@HashResult) AS NVARCHAR(10))

-- Test SHA256 (supported in SQL Server 2012+)
PRINT 'Testing SHA256...'
SET @HashResult = HASHBYTES('SHA256', @TestString)
IF @HashResult IS NULL
    PRINT '  SHA256: FAILED (returned NULL)'
ELSE
    PRINT '  SHA256: SUCCESS - Hash length: ' + CAST(LEN(@HashResult) AS NVARCHAR(10))

-- Test MD5 (supported in SQL Server 2005+)
PRINT 'Testing MD5...'
SET @HashResult = HASHBYTES('MD5', @TestString)
IF @HashResult IS NULL
    PRINT '  MD5: FAILED (returned NULL)'
ELSE
    PRINT '  MD5: SUCCESS - Hash length: ' + CAST(LEN(@HashResult) AS NVARCHAR(10))

-- =============================================
-- 3. TEST SPECIFIC PASSWORD HASHING SCENARIO
-- =============================================
PRINT ''
PRINT '3. TESTING PASSWORD HASHING SCENARIO:'

DECLARE @Password NVARCHAR(50) = 'Admin123!'
DECLARE @Salt NVARCHAR(50) = 'admin_salt_001'
DECLARE @CombinedString NVARCHAR(100) = @Password + @Salt

PRINT 'Password: ' + @Password
PRINT 'Salt: ' + @Salt
PRINT 'Combined: ' + @CombinedString

-- Test with SHA256
PRINT 'Testing SHA256 with password + salt:'
SET @HashResult = HASHBYTES('SHA256', @CombinedString)
IF @HashResult IS NULL
    PRINT '  SHA256: FAILED (returned NULL)'
ELSE
    PRINT '  SHA256: SUCCESS - Hash length: ' + CAST(LEN(@HashResult) AS NVARCHAR(10))

-- Test with SHA1
PRINT 'Testing SHA1 with password + salt:'
SET @HashResult = HASHBYTES('SHA1', @CombinedString)
IF @HashResult IS NULL
    PRINT '  SHA1: FAILED (returned NULL)'
ELSE
    PRINT '  SHA1: SUCCESS - Hash length: ' + CAST(LEN(@HashResult) AS NVARCHAR(10))

-- =============================================
-- 4. TEST DIFFERENT STRING CONCATENATION METHODS
-- =============================================
PRINT ''
PRINT '4. TESTING DIFFERENT STRING CONCATENATION METHODS:'

-- Method 1: + operator
PRINT 'Method 1: + operator'
SET @HashResult = HASHBYTES('SHA1', @Password + @Salt)
IF @HashResult IS NULL
    PRINT '  + operator: FAILED'
ELSE
    PRINT '  + operator: SUCCESS'

-- Method 2: CONCAT function
PRINT 'Method 2: CONCAT function'
SET @HashResult = HASHBYTES('SHA1', CONCAT(@Password, @Salt))
IF @HashResult IS NULL
    PRINT '  CONCAT: FAILED'
ELSE
    PRINT '  CONCAT: SUCCESS'

-- Method 3: Explicit casting
PRINT 'Method 3: Explicit casting'
SET @HashResult = HASHBYTES('SHA1', CAST(@Password + @Salt AS NVARCHAR(MAX)))
IF @HashResult IS NULL
    PRINT '  CAST: FAILED'
ELSE
    PRINT '  CAST: SUCCESS'

-- Method 4: String literal
PRINT 'Method 4: String literal'
SET @HashResult = HASHBYTES('SHA1', 'Admin123!admin_salt_001')
IF @HashResult IS NULL
    PRINT '  String literal: FAILED'
ELSE
    PRINT '  String literal: SUCCESS'

-- =============================================
-- 5. TEST CONVERT FUNCTION
-- =============================================
PRINT ''
PRINT '5. TESTING CONVERT FUNCTION:'

DECLARE @ConvertedHash NVARCHAR(255)

-- Test SHA1 with CONVERT
PRINT 'Testing SHA1 with CONVERT:'
SET @ConvertedHash = CONVERT(NVARCHAR(255), HASHBYTES('SHA1', @CombinedString), 2)
IF @ConvertedHash IS NULL
    PRINT '  SHA1 + CONVERT: FAILED'
ELSE
    PRINT '  SHA1 + CONVERT: SUCCESS - Length: ' + CAST(LEN(@ConvertedHash) AS NVARCHAR(10))

-- Test SHA256 with CONVERT
PRINT 'Testing SHA256 with CONVERT:'
SET @ConvertedHash = CONVERT(NVARCHAR(255), HASHBYTES('SHA256', @CombinedString), 2)
IF @ConvertedHash IS NULL
    PRINT '  SHA256 + CONVERT: FAILED'
ELSE
    PRINT '  SHA256 + CONVERT: SUCCESS - Length: ' + CAST(LEN(@ConvertedHash) AS NVARCHAR(10))

-- =============================================
-- 6. CHECK FOR NULL INPUT VALUES
-- =============================================
PRINT ''
PRINT '6. CHECKING FOR NULL INPUT VALUES:'

-- Check if any input is NULL
IF @Password IS NULL
    PRINT '  WARNING: Password is NULL'
ELSE
    PRINT '  Password is NOT NULL'

IF @Salt IS NULL
    PRINT '  WARNING: Salt is NULL'
ELSE
    PRINT '  Salt is NOT NULL'

IF @CombinedString IS NULL
    PRINT '  WARNING: Combined string is NULL'
ELSE
    PRINT '  Combined string is NOT NULL'

-- =============================================
-- 7. TEST WITH SIMPLE VALUES
-- =============================================
PRINT ''
PRINT '7. TESTING WITH SIMPLE VALUES:'

-- Test with simple string
PRINT 'Testing with simple string "hello":'
SET @HashResult = HASHBYTES('SHA1', 'hello')
IF @HashResult IS NULL
    PRINT '  Simple string: FAILED'
ELSE
    PRINT '  Simple string: SUCCESS'

-- Test with empty string
PRINT 'Testing with empty string:'
SET @HashResult = HASHBYTES('SHA1', '')
IF @HashResult IS NULL
    PRINT '  Empty string: FAILED'
ELSE
    PRINT '  Empty string: SUCCESS'

-- =============================================
-- 8. FINAL RECOMMENDATION
-- =============================================
PRINT ''
PRINT '8. FINAL RECOMMENDATION:'

-- Determine best working approach
IF HASHBYTES('SHA1', @CombinedString) IS NOT NULL
BEGIN
    PRINT '  RECOMMENDATION: Use SHA1 with + operator'
    PRINT '  Working example:'
    PRINT '  CONVERT(NVARCHAR(255), HASHBYTES(''SHA1'', ''Admin123!'' + ''admin_salt_001''), 2)'
END
ELSE IF HASHBYTES('SHA1', CONCAT(@Password, @Salt)) IS NOT NULL
BEGIN
    PRINT '  RECOMMENDATION: Use SHA1 with CONCAT function'
    PRINT '  Working example:'
    PRINT '  CONVERT(NVARCHAR(255), HASHBYTES(''SHA1'', CONCAT(''Admin123!'', ''admin_salt_001'')), 2)'
END
ELSE IF HASHBYTES('SHA1', 'Admin123!admin_salt_001') IS NOT NULL
BEGIN
    PRINT '  RECOMMENDATION: Use SHA1 with string literal'
    PRINT '  Working example:'
    PRINT '  CONVERT(NVARCHAR(255), HASHBYTES(''SHA1'', ''Admin123!admin_salt_001''), 2)'
END
ELSE
BEGIN
    PRINT '  ERROR: HASHBYTES function is not working at all!'
    PRINT '  This suggests a serious SQL Server configuration issue.'
    PRINT '  Contact your database administrator.'
END

PRINT ''
PRINT '=== HASHBYTES DIAGNOSTIC COMPLETE ==='

-- =============================================
-- 9. QUICK FIX SCRIPT
-- =============================================
PRINT ''
PRINT '9. QUICK FIX - Try this working INSERT statement:'

-- Show a working INSERT statement
PRINT 'INSERT INTO Users (FirstName, LastName, Email, Phone, PasswordHash, Salt, TenantId, Active, EmailVerified, PhoneVerified, LoginAttempts, AccountLocked, CreatedAt, UpdatedAt) VALUES ('
PRINT '    ''Admin'', ''User'', ''admin@example.com'', ''+1-555-0100'','
PRINT '    CONVERT(NVARCHAR(255), HASHBYTES(''SHA1'', ''Admin123!admin_salt_001''), 2),'
PRINT '    ''admin_salt_001'', 1, 1, 1, 1, 0, 0, GETUTCDATE(), GETUTCDATE()'
PRINT ');'

