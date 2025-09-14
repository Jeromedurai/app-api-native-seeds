-- auth
    -- token
-- User 
    -- login 
    -- reigister
    -- reset password
    -- logout
    -- get user
-- Profile
    -- Get user profile
    -- update user profile
-- product
    -- search product
    -- get product
    -- add product 
    -- update product
    -- delete product
-- image
    -- add image
    -- get images
-- category
    -- get category
    -- update category
    -- mernu master
-- cart
    -- add to cart
    -- update cart
    -- clear cart
    -- get cart
-- order
    -- get orders - admin
    -- get order by id or user
    -- order cancel
    -- order status update
    -- make order
-- wish list
    -- add to wish list
    -- update wish list
    -- clear wish list
    -- get wish list

-- Insert Application Configuration
INSERT INTO XC_APPCONFIG (ConfigKey, ConfigValue, Description, Category) VALUES
('MAX_ITEMS_IN_CART', '50', 'Maximum number of items allowed in shopping cart', 'Cart'),
('MAX_FILE_SIZE_MB', '10', 'Maximum file size for image uploads in MB', 'Upload'),
('DEFAULT_CURRENCY', 'USD', 'Default currency for the application', 'General'),
('DEFAULT_TIMEZONE', 'UTC', 'Default timezone for the application', 'General'),
('SESSION_TIMEOUT_MINUTES', '60', 'User session timeout in minutes', 'Security'),
('PASSWORD_EXPIRY_DAYS', '90', 'Password expiration period in days', 'Security'),
('MAX_LOGIN_ATTEMPTS', '5', 'Maximum failed login attempts before account lock', 'Security'),
('ACCOUNT_LOCK_DURATION_MINUTES', '30', 'Account lock duration in minutes', 'Security'),
('PASSWORD_RESET_TOKEN_EXPIRY_HOURS', '24', 'Password reset token expiry in hours', 'Security'),
('DEFAULT_PAGE_SIZE', '10', 'Default pagination page size', 'General'),
('MAX_PAGE_SIZE', '100', 'Maximum pagination page size', 'General'),
('ENABLE_EMAIL_VERIFICATION', 'true', 'Enable email verification for new users', 'Security'),
('ENABLE_PHONE_VERIFICATION', 'false', 'Enable phone verification for new users', 'Security'),
('DEFAULT_DELIVERY_DAYS', '7', 'Default delivery days for products', 'Shipping'),
('FREE_SHIPPING_THRESHOLD', '50.00', 'Minimum order amount for free shipping', 'Shipping');

-- Insert System Roles
INSERT INTO Roles (RoleName, RoleDescription, RoleLevel, IsSystemRole) VALUES
('SuperAdmin', 'Super Administrator with full system access', 10, 1),
('Admin', 'Administrator with management privileges', 8, 1),
('Manager', 'Manager with operational privileges', 6, 1),
('Executive', 'Executive with extended privileges', 4, 1),
('Support', 'Customer support representative', 3, 1),
('Customer', 'Regular customer user', 1, 1);

-- Insert System Permissions
INSERT INTO Permissions (PermissionName, PermissionDescription, PermissionCategory, ResourceType, ActionType) VALUES
-- Product Management
('view_products', 'View product catalog', 'Product Management', 'Product', 'View'),
('manage_products', 'Create, update, and delete products', 'Product Management', 'Product', 'Manage'),
('manage_product_images', 'Upload and manage product images', 'Product Management', 'Product', 'Manage'),
('view_inventory', 'View inventory levels and stock information', 'Inventory Management', 'Product', 'View'),
('manage_inventory', 'Manage inventory levels and stock', 'Inventory Management', 'Product', 'Manage'),

-- Order Management
('view_orders', 'View customer orders', 'Order Management', 'Order', 'View'),
('manage_orders', 'Manage order status and details', 'Order Management', 'Order', 'Manage'),
('process_refunds', 'Process order refunds and cancellations', 'Order Management', 'Order', 'Manage'),
('view_order_analytics', 'View order analytics and reports', 'Analytics', 'Order', 'View'),

-- User Management
('view_users', 'View user accounts and profiles', 'User Management', 'User', 'View'),
('manage_users', 'Create, update, and manage user accounts', 'User Management', 'User', 'Manage'),
('manage_user_roles', 'Assign and modify user roles', 'User Management', 'User', 'Manage'),
('view_user_activity', 'View user activity logs', 'User Management', 'User', 'View'),

-- Category Management
('view_categories', 'View product categories', 'Category Management', 'Category', 'View'),
('manage_categories', 'Create, update, and delete categories', 'Category Management', 'Category', 'Manage'),

-- Cart and Wishlist
('manage_cart', 'Add, remove, and modify cart items', 'Shopping', 'Cart', 'Manage'),
('manage_wishlist', 'Add, remove, and modify wishlist items', 'Shopping', 'Wishlist', 'Manage'),

-- Reports and Analytics
('view_reports', 'View system reports and analytics', 'Analytics', 'Report', 'View'),
('view_analytics', 'View detailed analytics and insights', 'Analytics', 'Analytics', 'View'),
('export_data', 'Export data and reports', 'Analytics', 'Data', 'Export'),

-- System Administration
('manage_settings', 'Manage system settings and configuration', 'System', 'Settings', 'Manage'),
('manage_roles', 'Create and modify system roles', 'System', 'Role', 'Manage'),
('view_system_logs', 'View system and audit logs', 'System', 'Log', 'View'),
('manage_notifications', 'Manage system notifications', 'System', 'Notification', 'Manage');

-- Insert Role-Permission Mappings
-- SuperAdmin - Full Access
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleName = 'SuperAdmin';

-- Admin - Management Access
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleName = 'Admin'
AND p.PermissionName IN (
	'view_products', 'manage_products', 'manage_product_images', 'view_inventory', 'manage_inventory',
	'view_orders', 'manage_orders', 'process_refunds', 'view_order_analytics',
	'view_users', 'manage_users', 'view_user_activity',
	'view_categories', 'manage_categories',
	'view_reports', 'view_analytics', 'export_data',
	'manage_notifications'
);

-- Manager - Operational Access
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleName = 'Manager'
AND p.PermissionName IN (
	'view_products', 'manage_products', 'view_inventory', 'manage_inventory',
	'view_orders', 'manage_orders', 'process_refunds',
	'view_users', 'view_categories', 'manage_categories',
	'view_reports', 'view_analytics'
);

-- Executive - Extended Customer Access
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleName = 'Executive'
AND p.PermissionName IN (
	'view_products', 'view_orders', 'manage_cart', 'manage_wishlist',
	'view_categories', 'view_inventory'
);

-- Support - Customer Service Access
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleName = 'Support'
AND p.PermissionName IN (
	'view_products', 'view_orders', 'view_users', 'view_user_activity',
	'view_categories', 'manage_notifications'
);

-- Customer - Basic Access
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM Roles r
CROSS JOIN Permissions p
WHERE r.RoleName = 'Customer'
AND p.PermissionName IN (
	'view_products', 'manage_cart', 'manage_wishlist', 'view_categories'
);

-- Insert Sample Categories
INSERT INTO Categories (CategoryName, Description, Active, OrderBy, Icon, TenantId) VALUES
('Electronics', 'Electronic devices and accessories', 1, 1, 'fa-laptop', 1),
('Clothing', 'Fashion and apparel', 1, 2, 'fa-tshirt', 1),
('Home & Garden', 'Home improvement and garden supplies', 1, 3, 'fa-home', 1),
('Sports & Outdoors', 'Sports equipment and outdoor gear', 1, 4, 'fa-football-ball', 1),
('Books & Media', 'Books, movies, and digital media', 1, 5, 'fa-book', 1),
('Health & Beauty', 'Health, wellness, and beauty products', 1, 6, 'fa-heart', 1),
('Food & Beverages', 'Food, drinks, and groceries', 1, 7, 'fa-utensils', 1),
('Automotive', 'Car parts and automotive accessories', 1, 8, 'fa-car', 1);

-- Insert Sample Menu Master
INSERT INTO MenuMaster (MenuName, OrderBy, Active, SubMenu, TenantId) VALUES
('Shop', 1, 1, 1, 1),
('Categories', 2, 1, 1, 1),
('Deals', 3, 1, 0, 1),
('New Arrivals', 4, 1, 0, 1),
('Best Sellers', 5, 1, 0, 1);

-- Link Categories to Menu
UPDATE Categories SET MenuId = (SELECT MenuId FROM MenuMaster WHERE MenuName = 'Categories' AND TenantId = 1)
WHERE TenantId = 1;

PRINT 'Database schema created successfully with optimized indexes and seed data.';

-- =============================================
-- USERS TABLE INSERT SCRIPT 
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



SELECT * FROM Users