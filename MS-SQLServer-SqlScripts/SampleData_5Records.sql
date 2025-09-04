-- Sample Data Insert Script with 5 Records for Each Table
-- Generated for Native Seed API Database

-- =============================================
-- 1. ROLES TABLE (XC_ROLE_MASTER)
-- =============================================
INSERT INTO XC_ROLE_MASTER (ROLE_ID, ROLE_NAME, ROLE_DESCRIPTION, ACTIVE, AUTHORITY_LEVEL, CREATED_ON, MODIFIED_ON)
VALUES 
('1001', 'Admin', 'Administrator with full access', 1, 1, GETDATE(), GETDATE()),
('1002', 'Manager', 'Manager with limited administrative access', 1, 2, GETDATE(), GETDATE()),
('1003', 'User', 'Regular user with basic access', 1, 3, GETDATE(), GETDATE()),
('1004', 'Guest', 'Guest user with read-only access', 1, 4, GETDATE(), GETDATE()),
('1005', 'Moderator', 'User with content moderation rights', 1, 2, GETDATE(), GETDATE());

-- =============================================
-- 2. USERS TABLE (XC_USER)
-- =============================================
INSERT INTO XC_USER (USER_ID, USER_NAME, FIRST_NAME, MIDDLE_NAME, LAST_NAME, LAST_LOGIN, ACTIVE, IS_SYSTEM_USER, CREATED_ON, MODIFIED_ON)
VALUES 
('2001', 'admin.user', 'Admin', 'A', 'User', GETDATE(), 1, 1, GETDATE(), GETDATE()),
('2002', 'john.doe', 'John', 'M', 'Doe', GETDATE(), 1, 0, GETDATE(), GETDATE()),
('2003', 'jane.smith', 'Jane', 'L', 'Smith', GETDATE(), 1, 0, GETDATE(), GETDATE()),
('2004', 'bob.wilson', 'Bob', 'R', 'Wilson', GETDATE(), 1, 0, GETDATE(), GETDATE()),
('2005', 'alice.brown', 'Alice', 'K', 'Brown', GETDATE(), 1, 0, GETDATE(), GETDATE());

-- =============================================
-- 3. USER ROLES TABLE (XC_USER_ROLES)
-- =============================================
INSERT INTO XC_USER_ROLES (USER_ROLE_ID, USER_ID, ROLE_ID, GROUP_ID, CREATED_ON, MODIFIED_ON)
VALUES 
('3001', '2001', '1001', 'G001', GETDATE(), GETDATE()),
('3002', '2002', '1002', 'G002', GETDATE(), GETDATE()),
('3003', '2003', '1003', 'G003', GETDATE(), GETDATE()),
('3004', '2004', '1003', 'G003', GETDATE(), GETDATE()),
('3005', '2005', '1004', 'G004', GETDATE(), GETDATE());

-- =============================================
-- 4. PRODUCT CATEGORIES TABLE (ProductCategory)
-- =============================================
INSERT INTO ProductCategory (CategoryId, Category, Link, SubCategory, OrderBy, SubMenu, Active, TenantId)
VALUES 
(1, 'Electronics', '/electronics', 0, 1, 1, 1, '1001'),
(2, 'Clothing', '/clothing', 0, 2, 1, 1, '1001'),
(3, 'Books', '/books', 0, 3, 1, 1, '1001'),
(4, 'Home & Garden', '/home-garden', 0, 4, 1, 1, '1001'),
(5, 'Sports', '/sports', 0, 5, 1, 1, '1001');

-- =============================================
-- 5. PRODUCT MASTER TABLE (ProductMaster)
-- =============================================
INSERT INTO ProductMaster (Id, ProductName, Displayname, Description, Rating, Price, Stock, Quantity, Numofreviews, Category, SubCategory, Active, TenantId, Created, LastModified)
VALUES 
(1, 'iPhone 15 Pro', 'iPhone 15 Pro 128GB', 'Latest iPhone with advanced features', 4.5, 999.99, 50, 1, 125, 1, 1, 1, '1001', GETDATE(), GETDATE()),
(2, 'Nike Air Max', 'Nike Air Max Running Shoes', 'Comfortable running shoes for athletes', 4.2, 129.99, 100, 1, 89, 5, 1, 1, '1001', GETDATE(), GETDATE()),
(3, 'The Great Gatsby', 'The Great Gatsby Novel', 'Classic American literature masterpiece', 4.8, 12.99, 200, 1, 256, 3, 1, 1, '1001', GETDATE(), GETDATE()),
(4, 'Coffee Maker', 'Premium Coffee Maker', 'Automatic coffee maker with timer', 4.1, 89.99, 75, 1, 67, 4, 1, 1, '1001', GETDATE(), GETDATE()),
(5, 'Denim Jacket', 'Classic Denim Jacket', 'Timeless denim jacket for all seasons', 4.3, 79.99, 60, 1, 94, 2, 1, 1, '1001', GETDATE(), GETDATE());

-- =============================================
-- 6. PRODUCT IMAGES TABLE (ProductImage)
-- =============================================
INSERT INTO ProductImage (Id, Url, Title, ProductId, Active)
VALUES 
(1, 'https://example.com/images/iphone15pro_1.jpg', 'iPhone 15 Pro Front View', 1, 1),
(2, 'https://example.com/images/iphone15pro_2.jpg', 'iPhone 15 Pro Back View', 1, 1),
(3, 'https://example.com/images/nike_airmax_1.jpg', 'Nike Air Max Side View', 2, 1),
(4, 'https://example.com/images/gatsby_book_1.jpg', 'The Great Gatsby Cover', 3, 1),
(5, 'https://example.com/images/coffee_maker_1.jpg', 'Coffee Maker Front View', 4, 1);

-- =============================================
-- 7. PRODUCT REVIEWS TABLE (ProductReview)
-- =============================================
INSERT INTO ProductReview (Id, Rating, Comment, ProductId, UserId, Active)
VALUES 
(1, 5, 'Excellent phone! The camera quality is amazing.', 1, 2002, 1),
(2, 4, 'Great running shoes, very comfortable for long runs.', 2, 2003, 1),
(3, 5, 'A timeless classic that everyone should read.', 3, 2004, 1),
(4, 4, 'Good coffee maker, makes delicious coffee every morning.', 4, 2005, 1),
(5, 4, 'Perfect fit and great quality denim.', 5, 2002, 1);

-- =============================================
-- 8. APP NOTIFICATIONS TABLE (XC_INAPP_NOTIFICATION)
-- =============================================
INSERT INTO XC_INAPP_NOTIFICATION (Id, Module, UserId, RoleId, Message, FromUser, Active)
VALUES 
(1, 'Product', 2002, '1003', 'New product iPhone 15 Pro is now available!', 2001, 1),
(2, 'Order', 2003, '1003', 'Your order #12345 has been shipped.', 2001, 1),
(3, 'System', 2004, '1003', 'System maintenance scheduled for tonight.', 2001, 1),
(4, 'Product', 2005, '1004', 'Price drop alert: Nike Air Max now 20% off!', 2001, 1),
(5, 'Account', 2002, '1003', 'Your account has been successfully verified.', 2001, 1);

-- =============================================
-- 9. NOTIFICATION MESSAGES TABLE (NotificationMessage)
-- =============================================
INSERT INTO NotificationMessage (MessageId, Message, Module, Active, CreatedOn, ViewedOn, Type)
VALUES 
(1, 'Welcome to our platform!', 'System', 1, GETDATE(), NULL, 'Info'),
(2, 'New features are available', 'System', 1, GETDATE(), NULL, 'Update'),
(3, 'Your order is ready for pickup', 'Order', 1, GETDATE(), NULL, 'Alert'),
(4, 'Payment received successfully', 'Payment', 1, GETDATE(), NULL, 'Success'),
(5, 'Please complete your profile', 'Account', 1, GETDATE(), NULL, 'Reminder');

-- =============================================
-- 10. APP CONFIG TABLE (XC_APPCONFIG)
-- =============================================
INSERT INTO XC_APPCONFIG (ID, CONFIGKEY, CONFIGVALUE)
VALUES 
(1, 'APP_NAME', 'Native Seed API'),
(2, 'VERSION', '1.0.0'),
(3, 'ENVIRONMENT', 'Development'),
(4, 'MAX_LOGIN_ATTEMPTS', '5'),
(5, 'SESSION_TIMEOUT', '30');

-- =============================================
-- 11. VALID TOKEN CONTEXT TABLE (ValidTokenContext)
-- =============================================
-- Note: This table only stores boolean values for token validation
-- Inserting sample validation results
INSERT INTO ValidTokenContext (ISVALID)
VALUES 
(1),  -- Valid token
(0),  -- Invalid token
(1),  -- Valid token
(0),  -- Invalid token
(1);  -- Valid token

-- =============================================
-- COMMIT TRANSACTION
-- =============================================
COMMIT;

PRINT 'Sample data inserted successfully! 5 records added to each table.';
