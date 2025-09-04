-- ====================================== 
-- COMPREHENSIVE E-COMMERCE DATABASE SCHEMA
-- Optimized for Performance and Scalability
-- ====================================== 

-- Drop existing tables in correct order (foreign key dependencies)
IF OBJECT_ID('UserCustomPermissions', 'U') IS NOT NULL DROP TABLE UserCustomPermissions;
IF OBJECT_ID('RolePermissions', 'U') IS NOT NULL DROP TABLE RolePermissions;
IF OBJECT_ID('UserRoles', 'U') IS NOT NULL DROP TABLE UserRoles;
IF OBJECT_ID('UserTokens', 'U') IS NOT NULL DROP TABLE UserTokens;
IF OBJECT_ID('UserSessions', 'U') IS NOT NULL DROP TABLE UserSessions;
IF OBJECT_ID('UserNotifications', 'U') IS NOT NULL DROP TABLE UserNotifications;
IF OBJECT_ID('UserActivityLog', 'U') IS NOT NULL DROP TABLE UserActivityLog;
IF OBJECT_ID('UserAddresses', 'U') IS NOT NULL DROP TABLE UserAddresses;
IF OBJECT_ID('UserPreferences', 'U') IS NOT NULL DROP TABLE UserPreferences;
IF OBJECT_ID('UserBehaviorAnalytics', 'U') IS NOT NULL DROP TABLE UserBehaviorAnalytics;
IF OBJECT_ID('PasswordResetTokens', 'U') IS NOT NULL DROP TABLE PasswordResetTokens;
IF OBJECT_ID('OrderTracking', 'U') IS NOT NULL DROP TABLE OrderTracking;
IF OBJECT_ID('OrderStatusHistory', 'U') IS NOT NULL DROP TABLE OrderStatusHistory;
IF OBJECT_ID('OrderRefunds', 'U') IS NOT NULL DROP TABLE OrderRefunds;
IF OBJECT_ID('OrderItems', 'U') IS NOT NULL DROP TABLE OrderItems;
IF OBJECT_ID('OrderAnalytics', 'U') IS NOT NULL DROP TABLE OrderAnalytics;
IF OBJECT_ID('Orders', 'U') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('NotificationQueue', 'U') IS NOT NULL DROP TABLE NotificationQueue;
IF OBJECT_ID('CartItems', 'U') IS NOT NULL DROP TABLE CartItems;
IF OBJECT_ID('ProductWishList', 'U') IS NOT NULL DROP TABLE ProductWishList;
IF OBJECT_ID('ProductReviews', 'U') IS NOT NULL DROP TABLE ProductReviews;
IF OBJECT_ID('ProductImages', 'U') IS NOT NULL DROP TABLE ProductImages;
IF OBJECT_ID('Products', 'U') IS NOT NULL DROP TABLE Products;
IF OBJECT_ID('Categories', 'U') IS NOT NULL DROP TABLE Categories;
IF OBJECT_ID('ProductCategories', 'U') IS NOT NULL DROP TABLE ProductCategories;
IF OBJECT_ID('MenuMaster', 'U') IS NOT NULL DROP TABLE MenuMaster;
IF OBJECT_ID('Permissions', 'U') IS NOT NULL DROP TABLE Permissions;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('XC_APPCONFIG', 'U') IS NOT NULL DROP TABLE XC_APPCONFIG;

-- ====================================== 
-- CORE SYSTEM TABLES
-- ====================================== 

-- Application Configuration Table
CREATE TABLE XC_APPCONFIG (
	ConfigId BIGINT IDENTITY(1,1) NOT NULL,
	ConfigKey NVARCHAR(255) NOT NULL,
	ConfigValue NVARCHAR(MAX) NULL,
	Description NVARCHAR(500) NULL,
	DataType NVARCHAR(50) DEFAULT 'String' NOT NULL,
	IsEncrypted BIT DEFAULT 0 NOT NULL,
	Category NVARCHAR(100) DEFAULT 'General' NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CreatedBy BIGINT NULL,
	UpdatedBy BIGINT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CONSTRAINT PK_XC_APPCONFIG PRIMARY KEY CLUSTERED (ConfigId),
	CONSTRAINT UQ_XC_APPCONFIG_ConfigKey UNIQUE (ConfigKey)
);

-- Users Table - Core user information
CREATE TABLE Users (
	UserId BIGINT IDENTITY(1,1) NOT NULL,
	FirstName NVARCHAR(100) NOT NULL,
	LastName NVARCHAR(100) NOT NULL,
	Email NVARCHAR(255) NOT NULL,
	Phone NVARCHAR(20) NULL,
	PasswordHash NVARCHAR(255) NOT NULL,
	Salt NVARCHAR(255) NOT NULL,
	TenantId BIGINT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	EmailVerified BIT DEFAULT 0 NOT NULL,
	PhoneVerified BIT DEFAULT 0 NOT NULL,
	LoginAttempts INT DEFAULT 0 NOT NULL,
	AccountLocked BIT DEFAULT 0 NOT NULL,
	LastLoginAttempt DATETIME2(7) NULL,
	LastLogin DATETIME2(7) NULL,
	LastLogout DATETIME2(7) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	-- Extended Profile Fields
	ProfilePicture NVARCHAR(500) NULL,
	DateOfBirth DATE NULL,
	Gender NVARCHAR(20) NULL,
	Timezone NVARCHAR(100) DEFAULT 'UTC' NULL,
	Language NVARCHAR(10) DEFAULT 'en' NULL,
	Country NVARCHAR(100) NULL,
	City NVARCHAR(100) NULL,
	State NVARCHAR(100) NULL,
	PostalCode NVARCHAR(20) NULL,
	Bio NVARCHAR(MAX) NULL,
	Website NVARCHAR(255) NULL,
	CompanyName NVARCHAR(255) NULL,
	JobTitle NVARCHAR(255) NULL,
	PreferredContactMethod NVARCHAR(50) DEFAULT 'Email' NULL,
	NotificationSettings NVARCHAR(MAX) NULL, -- JSON
	PrivacySettings NVARCHAR(MAX) NULL, -- JSON
	-- Security Fields
	PasswordChangedAt DATETIME2(7) NULL,
	LastPasswordReset DATETIME2(7) NULL,
	RememberMeToken NVARCHAR(255) NULL,
	RememberMeExpiry DATETIME2(7) NULL,
	AgreeToTerms BIT DEFAULT 0 NOT NULL,
	TermsAcceptedAt DATETIME2(7) NULL,
	CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
	CONSTRAINT UQ_Users_Email UNIQUE (Email),
	CONSTRAINT UQ_Users_Phone UNIQUE (Phone),
	CONSTRAINT CK_Users_LoginAttempts CHECK (LoginAttempts >= 0 AND LoginAttempts <= 10)
);

-- Roles Table - User roles and permissions
CREATE TABLE Roles (
	RoleId BIGINT IDENTITY(1,1) NOT NULL,
	RoleName NVARCHAR(50) NOT NULL,
	RoleDescription NVARCHAR(255) NULL,
	RoleLevel INT DEFAULT 1 NOT NULL, -- Hierarchy level
	IsSystemRole BIT DEFAULT 0 NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CreatedBy BIGINT NULL,
	UpdatedBy BIGINT NULL,
	CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (RoleId),
	CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName),
	CONSTRAINT CK_Roles_RoleLevel CHECK (RoleLevel >= 1 AND RoleLevel <= 10)
);

-- Permissions Table - System permissions
CREATE TABLE Permissions (
	PermissionId BIGINT IDENTITY(1,1) NOT NULL,
	PermissionName NVARCHAR(100) NOT NULL,
	PermissionDescription NVARCHAR(255) NULL,
	PermissionCategory NVARCHAR(100) DEFAULT 'General' NULL,
	ResourceType NVARCHAR(50) NULL, -- Product, Order, User, etc.
	ActionType NVARCHAR(50) NULL, -- View, Create, Update, Delete
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED (PermissionId),
	CONSTRAINT UQ_Permissions_PermissionName UNIQUE (PermissionName)
);

-- User Roles Junction Table
CREATE TABLE UserRoles (
	UserRoleId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	RoleId BIGINT NOT NULL,
	AssignedBy BIGINT NULL,
	AssignedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	ExpiresAt DATETIME2(7) NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserRoles PRIMARY KEY CLUSTERED (UserRoleId),
	CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
	CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
	CONSTRAINT FK_UserRoles_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(UserId),
	CONSTRAINT UQ_UserRoles_UserRole UNIQUE (UserId, RoleId, Active)
);

-- Role Permissions Junction Table
CREATE TABLE RolePermissions (
	RolePermissionId BIGINT IDENTITY(1,1) NOT NULL,
	RoleId BIGINT NOT NULL,
	PermissionId BIGINT NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED (RolePermissionId),
	CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
	CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId),
	CONSTRAINT UQ_RolePermissions_RolePermission UNIQUE (RoleId, PermissionId)
);

-- User Custom Permissions Table
CREATE TABLE UserCustomPermissions (
	UserCustomPermissionId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	PermissionId BIGINT NOT NULL,
	GrantedBy BIGINT NOT NULL,
	GrantedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	ExpiresAt DATETIME2(7) NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserCustomPermissions PRIMARY KEY CLUSTERED (UserCustomPermissionId),
	CONSTRAINT FK_UserCustomPermissions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
	CONSTRAINT FK_UserCustomPermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId),
	CONSTRAINT FK_UserCustomPermissions_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES Users(UserId),
	CONSTRAINT UQ_UserCustomPermissions_UserPermission UNIQUE (UserId, PermissionId)
);

-- ====================================== 
-- PRODUCT CATALOG TABLES
-- ====================================== 

-- Menu Master Table
CREATE TABLE MenuMaster (
	MenuId BIGINT IDENTITY(1,1) NOT NULL,
	MenuName NVARCHAR(255) NOT NULL,
	OrderBy INT DEFAULT 0 NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	Image NVARCHAR(500) NULL,
	SubMenu BIT DEFAULT 0 NOT NULL,
	TenantId BIGINT NULL,
	Created DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	Modified DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CreatedBy BIGINT NULL,
	ModifiedBy BIGINT NULL,
	CONSTRAINT PK_MenuMaster PRIMARY KEY CLUSTERED (MenuId)
);

-- Categories Table
CREATE TABLE Categories (
	CategoryId BIGINT IDENTITY(1,1) NOT NULL,
	CategoryName NVARCHAR(255) NOT NULL,
	Description NVARCHAR(MAX) NULL,
	Active BIT DEFAULT 1 NOT NULL,
	ParentCategoryId BIGINT NULL,
	OrderBy INT DEFAULT 0 NOT NULL,
	Icon NVARCHAR(255) NULL,
	HasSubMenu BIT DEFAULT 0 NOT NULL,
	Link NVARCHAR(500) NULL,
	TenantId BIGINT NULL,
	MenuId BIGINT NULL,
	Created DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	Modified DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CreatedBy BIGINT NULL,
	ModifiedBy BIGINT NULL,
	CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (CategoryId)
);

-- Product Categories Table (Alternative structure for flexibility)
CREATE TABLE ProductCategories (
	CategoryId BIGINT IDENTITY(1,1) NOT NULL,
	Category NVARCHAR(255) NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	OrderBy INT DEFAULT 0 NOT NULL,
	Description NVARCHAR(MAX) NULL,
	Icon NVARCHAR(255) NULL,
	SubMenu BIT DEFAULT 0 NOT NULL,
	TenantId BIGINT NULL,
	Created DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	Modified DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_ProductCategories PRIMARY KEY CLUSTERED (CategoryId)
);

-- Products Table - Core product information
CREATE TABLE Products (
	ProductId BIGINT IDENTITY(1,1) NOT NULL,
	TenantId BIGINT NULL,
	ProductName NVARCHAR(255) NOT NULL,
	ProductDescription NVARCHAR(500) NULL,
	ProductCode NVARCHAR(100) NOT NULL,
	FullDescription NVARCHAR(MAX) NULL,
	Specification NVARCHAR(MAX) NULL,
	Story NVARCHAR(MAX) NULL,
	PackQuantity INT DEFAULT 1 NOT NULL,
	Quantity INT DEFAULT 0 NOT NULL,
	Total INT DEFAULT 0 NOT NULL,
	Price DECIMAL(18,2) NOT NULL,
	Category BIGINT NULL,
	Rating DECIMAL(3,2) DEFAULT 0 NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	Trending INT DEFAULT 0 NOT NULL,
	UserBuyCount INT DEFAULT 0 NOT NULL,
	[Return] INT DEFAULT 30 NOT NULL, -- Return policy in days
	InStock BIT DEFAULT 0,
	BestSeller BIT DEFAULT 0 NOT NULL,
	DeliveryDate INT DEFAULT 7 NOT NULL, -- Delivery days
	Offer NVARCHAR(100) NULL,
	OrderBy INT DEFAULT 0 NOT NULL,
	UserId BIGINT NULL,
	Overview NVARCHAR(MAX) NULL,
	LongDescription NVARCHAR(MAX) NULL,
	-- SEO and Marketing
	MetaTitle NVARCHAR(255) NULL,
	MetaDescription NVARCHAR(500) NULL,
	MetaKeywords NVARCHAR(500) NULL,
	Slug NVARCHAR(255) NULL,
	-- Inventory Management
	SKU NVARCHAR(100) NULL,
	Barcode NVARCHAR(100) NULL,
	Weight DECIMAL(10,3) NULL,
	Dimensions NVARCHAR(100) NULL, -- L x W x H
	MinStockLevel INT DEFAULT 0 NOT NULL,
	MaxStockLevel INT NULL,
	ReorderPoint INT DEFAULT 0 NOT NULL,
	-- Pricing
	CostPrice DECIMAL(18,2) NULL,
	OriginalPrice DECIMAL(18,2) NULL,
	DiscountPercentage DECIMAL(5,2) DEFAULT 0 NOT NULL,
	-- Timestamps
	Created DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	Modified DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CreatedBy BIGINT NULL,
	ModifiedBy BIGINT NULL,
	DeletedAt DATETIME2(7) NULL,
	DeletedBy BIGINT NULL,
	CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (ProductId)
);

-- Product Images Table
CREATE TABLE ProductImages (
	ImageId BIGINT IDENTITY(1,1) NOT NULL,
	ProductId BIGINT NOT NULL,
	ImageName NVARCHAR(255) NOT NULL,
	ContentType NVARCHAR(100) NOT NULL,
	FileSize BIGINT NOT NULL,
	ImageData VARBINARY(MAX) NULL,
	ThumbnailData VARBINARY(MAX) NULL,
	Poster NVARCHAR(500) NULL, -- URL/Path
	Main BIT DEFAULT 0 NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	OrderBy INT DEFAULT 0 NOT NULL,
	AltText NVARCHAR(255) NULL,
	Caption NVARCHAR(500) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	Modified DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CreatedBy BIGINT NULL,
	ModifiedBy BIGINT NULL,
	DeletedAt DATETIME2(7) NULL,
	DeletedBy BIGINT NULL,
	CONSTRAINT PK_ProductImages PRIMARY KEY CLUSTERED (ImageId)
);

-- Product Reviews Table
CREATE TABLE ProductReviews (
	ReviewId BIGINT IDENTITY(1,1) NOT NULL,
	ProductId BIGINT NOT NULL,
	UserId BIGINT NOT NULL,
	Rating INT NOT NULL,
	ReviewTitle NVARCHAR(255) NULL,
	ReviewText NVARCHAR(MAX) NULL,
	IsVerifiedPurchase BIT DEFAULT 0 NOT NULL,
	IsApproved BIT DEFAULT 0 NOT NULL,
	HelpfulCount INT DEFAULT 0 NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	ApprovedAt DATETIME2(7) NULL,
	ApprovedBy BIGINT NULL,
	CONSTRAINT PK_ProductReviews PRIMARY KEY CLUSTERED (ReviewId)
);

-- Product Wishlist Table
CREATE TABLE ProductWishList (
	WishListId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	ProductId BIGINT NOT NULL,
	TenantId BIGINT NULL,
	Priority INT DEFAULT 0 NOT NULL,
	Notes NVARCHAR(500) NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_ProductWishList PRIMARY KEY CLUSTERED (WishListId)
);

-- ====================================== 
-- SHOPPING CART TABLES
-- ====================================== 

-- Cart Items Table
CREATE TABLE CartItems (
	CartId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	ProductId BIGINT NOT NULL,
	Quantity INT NOT NULL,
	TenantId BIGINT NULL,
	SessionId NVARCHAR(255) NULL,
	Active BIT DEFAULT 1 NOT NULL,
	AddedDate DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedDate DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	ExpiresAt DATETIME2(7) NULL, -- Cart expiration
	CONSTRAINT PK_CartItems PRIMARY KEY CLUSTERED (CartId)
);

-- ====================================== 
-- ORDER MANAGEMENT TABLES
-- ====================================== 

-- Orders Table
CREATE TABLE Orders (
	OrderId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	TenantId BIGINT NULL,
	OrderNumber NVARCHAR(50) NOT NULL,
	OrderStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
	PaymentStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
	TotalAmount DECIMAL(18,2) NOT NULL,
	Subtotal DECIMAL(18,2) NOT NULL,
	ShippingAmount DECIMAL(18,2) DEFAULT 0 NOT NULL,
	TaxAmount DECIMAL(18,2) DEFAULT 0 NOT NULL,
	DiscountAmount DECIMAL(18,2) DEFAULT 0 NOT NULL,
	CurrencyCode NVARCHAR(3) DEFAULT 'INR' NOT NULL,
	Notes NVARCHAR(1000) NULL,
	-- Address Information (JSON or normalized)
	ShippingAddress NVARCHAR(MAX) NULL, -- JSON
	BillingAddress NVARCHAR(MAX) NULL, -- JSON
	-- Payment and Shipping Information
	PaymentMethod NVARCHAR(MAX) NULL, -- JSON
	ShippingMethod NVARCHAR(MAX) NULL, -- JSON
	PaymentTransactionId NVARCHAR(255) NULL,
	ShippingTrackingNumber NVARCHAR(255) NULL,
	-- Important Timestamps
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	ShippedAt DATETIME2(7) NULL,
	DeliveredAt DATETIME2(7) NULL,
	CancelledAt DATETIME2(7) NULL,
	-- Cancellation Information
	CancelReason NVARCHAR(500) NULL,
	CancelledBy BIGINT NULL,
	-- System Fields
	Active BIT DEFAULT 1 NOT NULL,
	CreatedBy BIGINT NULL,
	UpdatedBy BIGINT NULL,
	CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (OrderId)
);

-- Order Items Table
CREATE TABLE OrderItems (
	OrderItemId BIGINT IDENTITY(1,1) NOT NULL,
	OrderId BIGINT NOT NULL,
	ProductId BIGINT NOT NULL,
	ProductName NVARCHAR(255) NOT NULL,
	ProductImage NVARCHAR(500) NULL,
	ProductCode NVARCHAR(100) NULL,
	Price DECIMAL(18,2) NOT NULL,
	OriginalPrice DECIMAL(18,2) NULL,
	Quantity INT NOT NULL,
	Total DECIMAL(18,2) NOT NULL,
	DiscountAmount DECIMAL(18,2) DEFAULT 0 NOT NULL,
	TaxAmount DECIMAL(18,2) DEFAULT 0 NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (OrderItemId),
	CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
	CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
	CONSTRAINT CK_OrderItems_Price CHECK (Price >= 0),
	CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
	CONSTRAINT CK_OrderItems_Total CHECK (Total >= 0)
);

-- Order Status History Table
CREATE TABLE OrderStatusHistory (
	StatusHistoryId BIGINT IDENTITY(1,1) NOT NULL,
	OrderId BIGINT NOT NULL,
	PreviousStatus NVARCHAR(50) NULL,
	NewStatus NVARCHAR(50) NOT NULL,
	StatusNote NVARCHAR(1000) NULL,
	ChangedBy BIGINT NULL,
	ChangedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_OrderStatusHistory PRIMARY KEY CLUSTERED (StatusHistoryId)
);

-- Order Tracking Table
CREATE TABLE OrderTracking (
	TrackingId BIGINT IDENTITY(1,1) NOT NULL,
	OrderId BIGINT NOT NULL,
	TrackingNumber NVARCHAR(100) NULL,
	Carrier NVARCHAR(100) NULL,
	TrackingStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
	EstimatedDelivery DATETIME2(7) NULL,
	ActualDelivery DATETIME2(7) NULL,
	TrackingUrl NVARCHAR(500) NULL,
	ShippingCost DECIMAL(18,2) NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_OrderTracking PRIMARY KEY CLUSTERED (TrackingId),
	CONSTRAINT FK_OrderTracking_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
	CONSTRAINT UQ_OrderTracking_OrderActive UNIQUE (OrderId, Active)
);

-- Order Refunds Table
CREATE TABLE OrderRefunds (
	RefundId BIGINT IDENTITY(1,1) NOT NULL,
	OrderId BIGINT NOT NULL,
	RefundAmount DECIMAL(18,2) NOT NULL,
	RefundReason NVARCHAR(500) NULL,
	RefundStatus NVARCHAR(50) DEFAULT 'Pending' NOT NULL,
	RefundMethod NVARCHAR(100) NULL,
	RefundTransactionId NVARCHAR(255) NULL,
	RequestedBy BIGINT NOT NULL,
	RequestedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	ProcessedAt DATETIME2(7) NULL,
	ProcessedBy BIGINT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_OrderRefunds PRIMARY KEY CLUSTERED (RefundId)
);

-- Order Analytics Table
CREATE TABLE OrderAnalytics (
	AnalyticsId BIGINT IDENTITY(1,1) NOT NULL,
	OrderId BIGINT NOT NULL,
	UserId BIGINT NOT NULL,
	OrderNumber NVARCHAR(50) NOT NULL,
	TotalAmount DECIMAL(18,2) NOT NULL,
	ItemCount INT NOT NULL,
	OrderDate DATETIME2(7) NOT NULL,
	TenantId BIGINT NULL,
	OrderSource NVARCHAR(50) DEFAULT 'Web' NULL, -- Web, Mobile, API
	DeviceType NVARCHAR(50) NULL,
	UserAgent NVARCHAR(500) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_OrderAnalytics PRIMARY KEY CLUSTERED (AnalyticsId)
);

-- ====================================== 
-- USER MANAGEMENT TABLES
-- ====================================== 

-- User Addresses Table
CREATE TABLE UserAddresses (
	AddressId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	AddressType NVARCHAR(50) DEFAULT 'Home' NOT NULL, -- Home, Work, Shipping, Billing
	Street NVARCHAR(255) NOT NULL,
	City NVARCHAR(100) NOT NULL,
	State NVARCHAR(100) NOT NULL,
	PostalCode NVARCHAR(20) NOT NULL,
	Country NVARCHAR(100) NOT NULL,
	IsDefault BIT DEFAULT 0 NOT NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserAddresses PRIMARY KEY CLUSTERED (AddressId)
);

-- User Preferences Table
CREATE TABLE UserPreferences (
	PreferenceId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	PreferenceKey NVARCHAR(100) NOT NULL,
	PreferenceValue NVARCHAR(MAX) NULL,
	PreferenceType NVARCHAR(50) DEFAULT 'String' NOT NULL, -- String, Number, Boolean, JSON
	Category NVARCHAR(100) DEFAULT 'General' NULL,
	Active BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserPreferences PRIMARY KEY CLUSTERED (PreferenceId)
);

-- User Tokens Table
CREATE TABLE UserTokens (
	TokenId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	Token NVARCHAR(MAX) NOT NULL,
	RefreshToken NVARCHAR(MAX) NULL,
	TokenType NVARCHAR(50) DEFAULT 'JWT' NOT NULL,
	DeviceId NVARCHAR(255) NULL,
	DeviceInfo NVARCHAR(500) NULL,
	IpAddress NVARCHAR(45) NULL,
	UserAgent NVARCHAR(500) NULL,
	ExpiresAt DATETIME2(7) NOT NULL,
	IsRevoked BIT DEFAULT 0 NOT NULL,
	RevokedAt DATETIME2(7) NULL,
	RevokedReason NVARCHAR(255) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserTokens PRIMARY KEY CLUSTERED (TokenId)
);

-- User Sessions Table
CREATE TABLE UserSessions (
	SessionId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	SessionToken NVARCHAR(255) NOT NULL,
	DeviceId NVARCHAR(255) NULL,
	IpAddress NVARCHAR(45) NULL,
	UserAgent NVARCHAR(500) NULL,
	LoginAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	LastActivityAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	LoggedOutAt DATETIME2(7) NULL,
	ExpiresAt DATETIME2(7) NOT NULL,
	IsActive BIT DEFAULT 1 NOT NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserSessions PRIMARY KEY CLUSTERED (SessionId)
);

-- Password Reset Tokens Table
CREATE TABLE PasswordResetTokens (
	ResetTokenId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	ResetToken NVARCHAR(255) NOT NULL,
	ExpiresAt DATETIME2(7) NOT NULL,
	IsUsed BIT DEFAULT 0 NOT NULL,
	UsedAt DATETIME2(7) NULL,
	IpAddress NVARCHAR(45) NULL,
	UserAgent NVARCHAR(500) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_PasswordResetTokens PRIMARY KEY CLUSTERED (ResetTokenId)
);

-- User Activity Log Table
CREATE TABLE UserActivityLog (
	ActivityLogId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	ActivityType NVARCHAR(100) NOT NULL,
	ActivityDescription NVARCHAR(MAX) NULL,
	IpAddress NVARCHAR(45) NULL,
	UserAgent NVARCHAR(500) NULL,
	DeviceId NVARCHAR(255) NULL,
	ResourceType NVARCHAR(50) NULL, -- Product, Order, User, etc.
	ResourceId BIGINT NULL,
	SessionId NVARCHAR(255) NULL,
	PerformedBy BIGINT NULL, -- For admin actions
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserActivityLog PRIMARY KEY CLUSTERED (ActivityLogId)
);

-- User Notifications Table
CREATE TABLE UserNotifications (
	NotificationId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	NotificationType NVARCHAR(50) NOT NULL,
	Title NVARCHAR(255) NOT NULL,
	Message NVARCHAR(MAX) NOT NULL,
	IsRead BIT DEFAULT 0 NOT NULL,
	ReadAt DATETIME2(7) NULL,
	Priority NVARCHAR(20) DEFAULT 'Normal' NOT NULL, -- Low, Normal, High, Critical
	ExpiresAt DATETIME2(7) NULL,
	ActionUrl NVARCHAR(500) NULL,
	ActionText NVARCHAR(100) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserNotifications PRIMARY KEY CLUSTERED (NotificationId)
);

-- Notification Queue Table
CREATE TABLE NotificationQueue (
	QueueId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	NotificationType NVARCHAR(50) NOT NULL,
	Subject NVARCHAR(255) NOT NULL,
	Message NVARCHAR(MAX) NOT NULL,
	OrderId BIGINT NULL,
	ProductId BIGINT NULL,
	Priority NVARCHAR(20) DEFAULT 'Normal' NOT NULL,
	Status NVARCHAR(50) DEFAULT 'Pending' NOT NULL, -- Pending, Sent, Failed
	ScheduledAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	SentAt DATETIME2(7) NULL,
	AttemptCount INT DEFAULT 0 NOT NULL,
	LastAttemptAt DATETIME2(7) NULL,
	ErrorMessage NVARCHAR(MAX) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	UpdatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_NotificationQueue PRIMARY KEY CLUSTERED (QueueId)
);

-- User Behavior Analytics Table
CREATE TABLE UserBehaviorAnalytics (
	BehaviorId BIGINT IDENTITY(1,1) NOT NULL,
	UserId BIGINT NOT NULL,
	ActionType NVARCHAR(100) NOT NULL,
	ActionDetails NVARCHAR(MAX) NULL,
	ItemCount INT NULL,
	TotalValue DECIMAL(18,2) NULL,
	SessionId NVARCHAR(255) NULL,
	IpAddress NVARCHAR(45) NULL,
	UserAgent NVARCHAR(500) NULL,
	DeviceType NVARCHAR(50) NULL,
	ReferrerUrl NVARCHAR(500) NULL,
	CreatedAt DATETIME2(7) DEFAULT GETUTCDATE() NOT NULL,
	CONSTRAINT PK_UserBehaviorAnalytics PRIMARY KEY CLUSTERED (BehaviorId)
);

-- ====================================== 
-- OPTIMIZED INDEX CREATION
-- Performance-focused indexing strategy
-- ====================================== 

-- ====================================== 
-- USERS TABLE INDEXES
-- ====================================== 

-- Primary lookup indexes
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users (Email) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_Users_Phone ON Users (Phone) WHERE Active = 1 AND Phone IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Users_TenantId ON Users (TenantId, Active, UserId);
CREATE NONCLUSTERED INDEX IX_Users_LastLogin ON Users (LastLogin DESC) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_Users_CreatedAt ON Users (CreatedAt DESC);

-- Authentication and security indexes
CREATE NONCLUSTERED INDEX IX_Users_LoginAttempts ON Users (AccountLocked, LoginAttempts, LastLoginAttempt) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_Users_EmailVerification ON Users (EmailVerified, PhoneVerified, Active);

-- Admin and reporting indexes
CREATE NONCLUSTERED INDEX IX_Users_ActiveStatus ON Users (Active, AccountLocked, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_Users_ProfileCompletion ON Users (DateOfBirth, Gender, Country, City) WHERE Active = 1;

-- ====================================== 
-- PRODUCTS TABLE INDEXES
-- ====================================== 

-- Primary product lookups
CREATE NONCLUSTERED INDEX IX_Products_TenantId_Active ON Products (TenantId, Active, ProductId);
CREATE NONCLUSTERED INDEX IX_Products_ProductCode ON Products (ProductCode, TenantId) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_Products_Category ON Products (Category, Active, Price, Rating DESC);
CREATE NONCLUSTERED INDEX IX_Products_SKU ON Products (SKU) WHERE Active = 1 AND SKU IS NOT NULL;

-- Search and filtering indexes
CREATE NONCLUSTERED INDEX IX_Products_Search ON Products (TenantId, Active) INCLUDE (ProductName, ProductDescription, ProductCode, Price, Rating);
CREATE NONCLUSTERED INDEX IX_Products_Price ON Products (TenantId, Active, Price ASC);
CREATE NONCLUSTERED INDEX IX_Products_Rating ON Products (TenantId, Active, Rating DESC);
CREATE NONCLUSTERED INDEX IX_Products_Stock ON Products (TenantId, Active, Quantity DESC);
CREATE NONCLUSTERED INDEX IX_Products_BestSeller ON Products (TenantId, Active, BestSeller DESC, UserBuyCount DESC);
CREATE NONCLUSTERED INDEX IX_Products_Trending ON Products (TenantId, Active, Trending DESC, Created DESC);

-- Sorting and ordering indexes
CREATE NONCLUSTERED INDEX IX_Products_Created ON Products (TenantId, Active, Created DESC);
CREATE NONCLUSTERED INDEX IX_Products_Modified ON Products (TenantId, Active, Modified DESC);
CREATE NONCLUSTERED INDEX IX_Products_UserBuyCount ON Products (TenantId, Active, UserBuyCount DESC);
CREATE NONCLUSTERED INDEX IX_Products_OrderBy ON Products (TenantId, Active, OrderBy ASC, ProductName ASC);

-- Inventory management indexes
CREATE NONCLUSTERED INDEX IX_Products_Inventory ON Products (Active, Quantity, MinStockLevel, ReorderPoint) WHERE Active = 1;
--CREATE NONCLUSTERED INDEX IX_Products_LowStock ON Products (Active, MinStockLevel, Quantity) 
--WHERE Active = 1 AND Quantity <= MinStockLevel;

-- ====================================== 
-- PRODUCT IMAGES TABLE INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_ProductImages_ProductId ON ProductImages (ProductId, Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_ProductImages_Main ON ProductImages (ProductId, Main DESC, Active) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_ProductImages_Active ON ProductImages (Active, CreatedAt DESC);

-- ====================================== 
-- CATEGORIES TABLE INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_Categories_TenantId ON Categories (TenantId, Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_Categories_ParentId ON Categories (ParentCategoryId, Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_Categories_MenuId ON Categories (MenuId, Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_Categories_Name ON Categories (CategoryName, TenantId) WHERE Active = 1;

-- ====================================== 
-- ORDERS TABLE INDEXES
-- ====================================== 

-- Primary order lookups
CREATE NONCLUSTERED INDEX IX_Orders_UserId ON Orders (UserId, Active, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_Orders_OrderNumber ON Orders (OrderNumber) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_Orders_TenantId ON Orders (TenantId, Active, CreatedAt DESC);

-- Status and payment indexes
CREATE NONCLUSTERED INDEX IX_Orders_Status ON Orders (OrderStatus, Active, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_Orders_PaymentStatus ON Orders (PaymentStatus, Active, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_Orders_StatusPayment ON Orders (OrderStatus, PaymentStatus, Active);

-- Date-based indexes for reporting
CREATE NONCLUSTERED INDEX IX_Orders_CreatedAt ON Orders (CreatedAt DESC, Active) INCLUDE (TotalAmount, OrderStatus, PaymentStatus);
CREATE NONCLUSTERED INDEX IX_Orders_UpdatedAt ON Orders (UpdatedAt DESC) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_Orders_ShippedAt ON Orders (ShippedAt DESC) WHERE ShippedAt IS NOT NULL AND Active = 1;
CREATE NONCLUSTERED INDEX IX_Orders_DeliveredAt ON Orders (DeliveredAt DESC) WHERE DeliveredAt IS NOT NULL AND Active = 1;

-- Admin and analytics indexes
CREATE NONCLUSTERED INDEX IX_Orders_TotalAmount ON Orders (TotalAmount DESC, CreatedAt DESC) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_Orders_Analytics ON Orders (TenantId, OrderStatus, PaymentStatus, CreatedAt DESC) INCLUDE (TotalAmount, UserId);

-- ====================================== 
-- ORDER ITEMS TABLE INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId ON OrderItems (OrderId, Active);
CREATE NONCLUSTERED INDEX IX_OrderItems_ProductId ON OrderItems (ProductId, Active, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_OrderItems_OrderProduct ON OrderItems (OrderId, ProductId) WHERE Active = 1;

-- ====================================== 
-- CART ITEMS TABLE INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_CartItems_UserId ON CartItems (UserId, Active, AddedDate DESC);
CREATE NONCLUSTERED INDEX IX_CartItems_ProductId ON CartItems (ProductId, Active);
CREATE NONCLUSTERED INDEX IX_CartItems_TenantId ON CartItems (TenantId, Active) WHERE TenantId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_CartItems_SessionId ON CartItems (SessionId, Active) WHERE SessionId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_CartItems_UserProduct ON CartItems (UserId, ProductId, Active);
CREATE NONCLUSTERED INDEX IX_CartItems_Expiration ON CartItems (ExpiresAt ASC) WHERE ExpiresAt IS NOT NULL AND Active = 1;

-- ====================================== 
-- USER ROLES AND PERMISSIONS INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_UserRoles_UserId ON UserRoles (UserId, Active);
CREATE NONCLUSTERED INDEX IX_UserRoles_RoleId ON UserRoles (RoleId, Active);
CREATE NONCLUSTERED INDEX IX_UserRoles_AssignedBy ON UserRoles (AssignedBy, AssignedAt DESC) WHERE AssignedBy IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_RolePermissions_RoleId ON RolePermissions (RoleId, Active);
CREATE NONCLUSTERED INDEX IX_RolePermissions_PermissionId ON RolePermissions (PermissionId, Active);

CREATE NONCLUSTERED INDEX IX_UserCustomPermissions_UserId ON UserCustomPermissions (UserId, Active);
CREATE NONCLUSTERED INDEX IX_UserCustomPermissions_PermissionId ON UserCustomPermissions (PermissionId, Active);

-- ====================================== 
-- ACTIVITY AND LOGGING INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_UserActivityLog_UserId ON UserActivityLog (UserId, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_UserActivityLog_ActivityType ON UserActivityLog (ActivityType, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_UserActivityLog_ResourceType ON UserActivityLog (ResourceType, ResourceId, CreatedAt DESC) WHERE ResourceType IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_UserActivityLog_PerformedBy ON UserActivityLog (PerformedBy, CreatedAt DESC) WHERE PerformedBy IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_UserActivityLog_CreatedAt ON UserActivityLog (CreatedAt DESC) INCLUDE (UserId, ActivityType, ActivityDescription);

-- ====================================== 
-- ORDER TRACKING AND HISTORY INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_OrderStatusHistory_OrderId ON OrderStatusHistory (OrderId, ChangedAt DESC);
CREATE NONCLUSTERED INDEX IX_OrderStatusHistory_ChangedBy ON OrderStatusHistory (ChangedBy, ChangedAt DESC) WHERE ChangedBy IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_OrderTracking_OrderId ON OrderTracking (OrderId, Active);
CREATE NONCLUSTERED INDEX IX_OrderTracking_TrackingNumber ON OrderTracking (TrackingNumber) WHERE TrackingNumber IS NOT NULL AND Active = 1;
CREATE NONCLUSTERED INDEX IX_OrderTracking_Status ON OrderTracking (TrackingStatus, UpdatedAt DESC) WHERE Active = 1;

CREATE NONCLUSTERED INDEX IX_OrderRefunds_OrderId ON OrderRefunds (OrderId, Active);
CREATE NONCLUSTERED INDEX IX_OrderRefunds_Status ON OrderRefunds (RefundStatus, RequestedAt DESC) WHERE Active = 1;
CREATE NONCLUSTERED INDEX IX_OrderRefunds_RequestedBy ON OrderRefunds (RequestedBy, RequestedAt DESC);

-- ====================================== 
-- NOTIFICATIONS AND QUEUE INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_UserNotifications_UserId ON UserNotifications (UserId, IsRead, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_UserNotifications_Type ON UserNotifications (NotificationType, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_UserNotifications_Unread ON UserNotifications (UserId, IsRead, Priority DESC, CreatedAt DESC) WHERE IsRead = 0;

CREATE NONCLUSTERED INDEX IX_NotificationQueue_Status ON NotificationQueue (Status, ScheduledAt ASC) WHERE Status = 'Pending';
CREATE NONCLUSTERED INDEX IX_NotificationQueue_UserId ON NotificationQueue (UserId, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_NotificationQueue_Type ON NotificationQueue (NotificationType, Status, CreatedAt DESC);

-- ====================================== 
-- ANALYTICS AND REPORTING INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_OrderAnalytics_UserId ON OrderAnalytics (UserId, OrderDate DESC);
CREATE NONCLUSTERED INDEX IX_OrderAnalytics_TenantId ON OrderAnalytics (TenantId, OrderDate DESC) WHERE TenantId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_OrderAnalytics_OrderDate ON OrderAnalytics (OrderDate DESC) INCLUDE (TotalAmount, ItemCount, UserId);
CREATE NONCLUSTERED INDEX IX_OrderAnalytics_OrderSource ON OrderAnalytics (OrderSource, OrderDate DESC) WHERE OrderSource IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_UserBehaviorAnalytics_UserId ON UserBehaviorAnalytics (UserId, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_UserBehaviorAnalytics_ActionType ON UserBehaviorAnalytics (ActionType, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_UserBehaviorAnalytics_SessionId ON UserBehaviorAnalytics (SessionId, CreatedAt DESC) WHERE SessionId IS NOT NULL;

-- ====================================== 
-- USER MANAGEMENT INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_UserAddresses_UserId ON UserAddresses (UserId, Active, IsDefault DESC);
CREATE NONCLUSTERED INDEX IX_UserAddresses_Type ON UserAddresses (UserId, AddressType, Active);

CREATE NONCLUSTERED INDEX IX_UserPreferences_UserId ON UserPreferences (UserId, Active, Category);
CREATE NONCLUSTERED INDEX IX_UserPreferences_Key ON UserPreferences (PreferenceKey, Active);

CREATE NONCLUSTERED INDEX IX_UserTokens_UserId ON UserTokens (UserId, IsRevoked, ExpiresAt DESC);
CREATE NONCLUSTERED INDEX IX_UserTokens_DeviceId ON UserTokens (DeviceId, IsRevoked) WHERE DeviceId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_UserTokens_ExpiresAt ON UserTokens (ExpiresAt ASC, IsRevoked) WHERE IsRevoked = 0;

CREATE NONCLUSTERED INDEX IX_UserSessions_UserId ON UserSessions (UserId, IsActive, LastActivityAt DESC);
CREATE NONCLUSTERED INDEX IX_UserSessions_SessionToken ON UserSessions (SessionToken, IsActive);
CREATE NONCLUSTERED INDEX IX_UserSessions_DeviceId ON UserSessions (DeviceId, IsActive) WHERE DeviceId IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_UserId ON PasswordResetTokens (UserId, IsUsed, ExpiresAt DESC);
CREATE NONCLUSTERED INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens (ResetToken, IsUsed, ExpiresAt);

-- ====================================== 
-- MENU AND CATEGORIES INDEXES
-- ====================================== 

CREATE NONCLUSTERED INDEX IX_MenuMaster_TenantId ON MenuMaster (TenantId, Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_MenuMaster_OrderBy ON MenuMaster (OrderBy ASC, MenuName ASC) WHERE Active = 1;

CREATE NONCLUSTERED INDEX IX_Categories_TenantId_Active ON Categories (TenantId, Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_Categories_ParentId_Active ON Categories (ParentCategoryId, Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_Categories_MenuId_Active ON Categories (MenuId, Active, OrderBy ASC) WHERE MenuId IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_ProductCategories_Active ON ProductCategories (Active, OrderBy ASC);
CREATE NONCLUSTERED INDEX IX_ProductCategories_TenantId ON ProductCategories (TenantId, Active) WHERE TenantId IS NOT NULL;

-- ====================================== 
-- PERFORMANCE OPTIMIZATION INDEXES
-- ====================================== 

-- Composite indexes for complex queries
CREATE NONCLUSTERED INDEX IX_Products_SearchFilter ON Products (TenantId, Active, Category, BestSeller, Rating DESC) 
	INCLUDE (ProductId, ProductName, Price, Quantity, UserBuyCount, Offer);

CREATE NONCLUSTERED INDEX IX_Products_PriceRange ON Products (TenantId, Active, Price ASC) 
	INCLUDE (ProductId, ProductName, Category, Rating, BestSeller);

CREATE NONCLUSTERED INDEX IX_Orders_UserStatus ON Orders (UserId, OrderStatus, Active, CreatedAt DESC) 
	INCLUDE (OrderId, OrderNumber, TotalAmount, PaymentStatus);

--CREATE NONCLUSTERED INDEX IX_Orders_AdminView ON Orders (TenantId, OrderStatus, CreatedAt DESC) 
--	INCLUDE (OrderId, OrderNumber, UserId, TotalAmount, PaymentStatus, ItemCount);

-- Cart optimization indexes
CREATE NONCLUSTERED INDEX IX_CartItems_UserTenant ON CartItems (UserId, TenantId, Active, AddedDate DESC) 
	INCLUDE (ProductId, Quantity);

-- Analytics optimization indexes
CREATE NONCLUSTERED INDEX IX_UserActivityLog_Analytics ON UserActivityLog (UserId, ActivityType, CreatedAt DESC) 
	WHERE ActivityType IN ('LOGIN', 'ORDER_CREATED', 'PRODUCT_VIEW', 'CART_ADD');

-- ====================================== 
-- FULL-TEXT SEARCH INDEXES (Optional - for advanced search)
-- ====================================== 

-- Enable full-text search on products
-- CREATE FULLTEXT CATALOG ProductSearchCatalog;
-- CREATE FULLTEXT INDEX ON Products (ProductName, ProductDescription, FullDescription, Overview, LongDescription)
--     KEY INDEX PK_Products ON ProductSearchCatalog;

-- ====================================== 
-- COMPUTED COLUMNS AND CONSTRAINTS
-- ====================================== 

-- Add computed columns for better performance
--ALTER TABLE Products ADD FullName AS (ProductName + ' ' + ISNULL(ProductCode, '')) PERSISTED;
--ALTER TABLE Products ADD PriceWithDiscount AS (Price * (1 - DiscountPercentage / 100)) PERSISTED;
--ALTER TABLE Users ADD FullName AS (FirstName + ' ' + LastName) PERSISTED;
--ALTER TABLE Orders ADD OrderAge AS DATEDIFF(DAY, CreatedAt, GETUTCDATE()) PERSISTED;

---- Create indexes on computed columns
--CREATE NONCLUSTERED INDEX IX_Products_FullName ON Products (FullName) WHERE Active = 1;
--CREATE NONCLUSTERED INDEX IX_Products_PriceWithDiscount ON Products (TenantId, Active, PriceWithDiscount ASC);
--CREATE NONCLUSTERED INDEX IX_Users_FullName ON Users (FullName) WHERE Active = 1;
--CREATE NONCLUSTERED INDEX IX_Orders_OrderAge ON Orders (OrderAge ASC, OrderStatus) WHERE Active = 1;