-- ====================================== 
-- SEED DATA INSERTION
-- Essential data for system operation
-- ====================================== 

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