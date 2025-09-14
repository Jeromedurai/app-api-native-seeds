USE [DB_HIMALAYA]
IF OBJECT_ID(N'[dbo].[SP_USER_LOGOUT]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_USER_LOGOUT];
GO

IF OBJECT_ID(N'[dbo].[SP_ADD_PRODUCT]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ADD_PRODUCT];
GO

IF OBJECT_ID(N'[dbo].[SP_DELETE_PRODUCT]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_DELETE_PRODUCT];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_MENU_MASTER]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_MENU_MASTER];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_PRODUCT_BY_ID]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_PRODUCT_BY_ID];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_USER_CART]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_USER_CART];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_USER_PROFILE]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_USER_PROFILE];
GO

IF OBJECT_ID(N'[dbo].[SP_RESET_PASSWORD]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_RESET_PASSWORD];
GO

IF OBJECT_ID(N'[dbo].[SP_SEARCH_PRODUCTS]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_SEARCH_PRODUCTS];
GO

IF OBJECT_ID(N'[dbo].[SP_UPDATE_PRODUCT]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_UPDATE_PRODUCT];
GO

IF OBJECT_ID(N'[dbo].[SP_UPDATE_USER_PROFILE]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_UPDATE_USER_PROFILE];
GO

IF OBJECT_ID(N'[dbo].[SP_USER_LOGIN]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_USER_LOGIN];
GO

IF OBJECT_ID(N'[dbo].[SP_USER_REGISTER]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_USER_REGISTER];
GO

IF OBJECT_ID(N'[dbo].[SP_ADD_ITEM_TO_CART]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ADD_ITEM_TO_CART];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_USER_CART]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_USER_CART];
GO

IF OBJECT_ID(N'[dbo].[SP_REMOVE_ITEM_FROM_CART]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_REMOVE_ITEM_FROM_CART];
GO

IF OBJECT_ID(N'[dbo].[SP_CLEAR_CART]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_CLEAR_CART];
GO

IF OBJECT_ID(N'[dbo].[SP_CREATE_ORDER]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_CREATE_ORDER];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_ORDERS]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_ORDERS];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_ORDER_BY_ID]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_ORDER_BY_ID];
GO

IF OBJECT_ID(N'[dbo].[SP_CREATE_ORDER]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_CREATE_ORDER];
GO

IF OBJECT_ID(N'[dbo].[SP_CANCEL_ORDER]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_CANCEL_ORDER];
GO

IF OBJECT_ID(N'[dbo].[SP_UPDATE_ORDER_STATUS]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_UPDATE_ORDER_STATUS];
GO

IF OBJECT_ID(N'[dbo].[SP_ADMIN_GET_ALL_USERS]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ADMIN_GET_ALL_USERS];
GO

IF OBJECT_ID(N'[dbo].[SP_ADMIN_UPDATE_USER_ROLE]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ADMIN_UPDATE_USER_ROLE];
GO

IF OBJECT_ID(N'[dbo].[SP_ADMIN_GET_ALL_ORDERS]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ADMIN_GET_ALL_ORDERS];
GO

IF OBJECT_ID(N'[dbo].[SP_ADD_PRODUCT_IMAGES]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ADD_PRODUCT_IMAGES];
GO

IF OBJECT_ID(N'[dbo].[SP_UPDATE_PRODUCT_IMAGE]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_UPDATE_PRODUCT_IMAGE];
GO

IF OBJECT_ID(N'[dbo].[SP_DELETE_PRODUCT_IMAGE]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_DELETE_PRODUCT_IMAGE];
GO

IF OBJECT_ID(N'[dbo].[SP_GET_ALL_CATEGORIES]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_GET_ALL_CATEGORIES];
GO

IF OBJECT_ID(N'[dbo].[SP_ADD_CATEGORY]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_ADD_CATEGORY];
GO

IF OBJECT_ID(N'[dbo].[SP_UPDATE_CATEGORY]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_UPDATE_CATEGORY];
GO


CREATE PROCEDURE [dbo].[SP_UPDATE_CATEGORY]
	@CategoryId BIGINT,
	@TenantId BIGINT,
	@CategoryName NVARCHAR(255),
	@Description NVARCHAR(MAX) = NULL,
	@Active BIT = 1,
	@ParentCategoryId BIGINT = NULL,
	@OrderBy INT = 0,
	@Icon NVARCHAR(255) = NULL,
	@HasSubMenu BIT = 0,
	@Link NVARCHAR(500) = NULL,
	@UserId BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		BEGIN TRANSACTION;
		
		-- Check if category exists and belongs to the tenant
		IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryId = @CategoryId AND TenantId = @TenantId)
		BEGIN
			RAISERROR('Category not found or does not belong to this tenant.', 16, 1);
			RETURN;
		END
		
		-- Check if new category name already exists for this tenant (excluding current category)
		IF EXISTS (SELECT 1 FROM Categories WHERE CategoryName = @CategoryName AND TenantId = @TenantId AND CategoryId != @CategoryId)
		BEGIN
			RAISERROR('Category name already exists for this tenant.', 16, 1);
			RETURN;
		END
		
		-- Validate parent category if provided
		IF @ParentCategoryId IS NOT NULL
		BEGIN
			IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryId = @ParentCategoryId AND TenantId = @TenantId)
			BEGIN
				RAISERROR('Parent category does not exist.', 16, 1);
				RETURN;
			END
			
			-- Prevent circular reference (category cannot be its own parent or grandparent)
			IF @ParentCategoryId = @CategoryId
			BEGIN
				RAISERROR('Category cannot be its own parent.', 16, 1);
				RETURN;
			END
		END
		
		-- Update category
		UPDATE Categories
		SET 
			CategoryName = @CategoryName,
			Description = @Description,
			Active = @Active,
			ParentCategoryId = @ParentCategoryId,
			OrderBy = @OrderBy,
			Icon = @Icon,
			HasSubMenu = @HasSubMenu,
			Link = @Link,
			Modified = GETUTCDATE(),
			ModifiedBy = @UserId
		WHERE CategoryId = @CategoryId
			AND TenantId = @TenantId;
		
		-- Return the updated category ID
		SELECT @CategoryId AS CategoryId;
		
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
			
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_ADD_CATEGORY]
	@TenantId BIGINT,
	@CategoryName NVARCHAR(255),
	@Description NVARCHAR(MAX) = NULL,
	@Active BIT = 1,
	@ParentCategoryId BIGINT = NULL,
	@OrderBy INT = 0,
	@Icon NVARCHAR(255) = NULL,
	@HasSubMenu BIT = 0,
	@Link NVARCHAR(500) = NULL,
	@UserId BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		BEGIN TRANSACTION;
		
		-- Check if category name already exists for this tenant
		IF EXISTS (SELECT 1 FROM Categories WHERE CategoryName = @CategoryName AND TenantId = @TenantId)
		BEGIN
			RAISERROR('Category name already exists for this tenant.', 16, 1);
			RETURN;
		END
		
		-- Validate parent category if provided
		IF @ParentCategoryId IS NOT NULL
		BEGIN
			IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryId = @ParentCategoryId AND TenantId = @TenantId)
			BEGIN
				RAISERROR('Parent category does not exist.', 16, 1);
				RETURN;
			END
		END
		
		-- Insert new category
		INSERT INTO Categories (
			TenantId,
			CategoryName,
			Description,
			Active,
			ParentCategoryId,
			OrderBy,
			Icon,
			HasSubMenu,
			Link,
			Created,
			CreatedBy,
			Modified,
			ModifiedBy
		) VALUES (
			@TenantId,
			@CategoryName,
			@Description,
			@Active,
			@ParentCategoryId,
			@OrderBy,
			@Icon,
			@HasSubMenu,
			@Link,
			GETUTCDATE(),
			@UserId,
			GETUTCDATE(),
			@UserId
		);
		
		-- Return the new category ID
		SELECT SCOPE_IDENTITY() AS CategoryId;
		
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
			
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_GET_ALL_CATEGORIES]
	@TenantId BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Get all categories with optional tenant filtering
		SELECT 
			c.CategoryId,
			c.CategoryName AS Category,
			c.Active,
			c.HasSubMenu AS SubMenu,
			c.Created,
			c.Modified,
			c.OrderBy,
			c.Description,
			c.Icon,
			c.ParentCategoryId,
			c.TenantId
		FROM Categories c
		WHERE (@TenantId IS NULL OR c.TenantId = @TenantId)
			AND c.Active = 1
		ORDER BY c.OrderBy, c.CategoryName;
		
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_USER_LOGOUT]
	@UserId BIGINT,
	@DeviceId NVARCHAR(255) = NULL,
	@LogoutFromAllDevices BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		BEGIN TRANSACTION;
		
		-- Validate user exists and is active
		IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND Active = 1)
		BEGIN
			RAISERROR('User not found or inactive.', 16, 1);
			RETURN;
		END
		
		-- If logout from all devices is requested
		IF @LogoutFromAllDevices = 1
		BEGIN
			-- Clear remember me sessions
			UPDATE Users 
			SET RememberMeToken = NULL,
				RememberMeExpiry = NULL,
				LastLogout = GETUTCDATE(),
				LastLogin = NULL
			WHERE UserId = @UserId;
		END
		ELSE
		BEGIN
			-- Update last logout time
			UPDATE Users 
			SET LastLogout = GETUTCDATE(),
			LastLogin = NULL
			WHERE UserId = @UserId;
		END
		
		-- Log the logout activity
		INSERT INTO UserActivityLog (
			UserId,
			ActivityType,
			ActivityDescription,
			IPAddress,
			UserAgent,
			DeviceId,
			CreatedAt
		) VALUES (
			@UserId,
			'LOGOUT',
			CASE 
			WHEN @LogoutFromAllDevices = 1 THEN 'Logout from all devices'
				WHEN @DeviceId IS NOT NULL THEN 'Logout from device: ' + @DeviceId
				ELSE 'User logout'
			END,
			NULL, -- IP Address would be passed from application
			NULL, -- User Agent would be passed from application
			@DeviceId,
			GETUTCDATE()
		);
		
		-- Return success status
		SELECT 
			@UserId AS UserId,
			'Logged out successfully' AS Message,
			GETUTCDATE() AS LogoutTime,
			@LogoutFromAllDevices AS LogoutFromAllDevices;
		
		COMMIT TRANSACTION;
		
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
			
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_GET_MENU_MASTER]
	@TenantId BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Get menu master with categories
		SELECT 
			m.MenuId,
			m.MenuName,
			m.OrderBy,
			m.Active,
			m.Image,
			m.SubMenu,
			m.TenantId,
			m.Created,
			m.Modified,
			-- Category information
			c.CategoryId,
			c.CategoryName AS Category,
			c.Active AS CategoryActive,
			c.OrderBy AS CategoryOrderBy,
			c.Icon AS CategoryIcon,
			c.Description AS CategoryDescription
		FROM MenuMaster m
		LEFT JOIN Categories c ON m.MenuId = c.MenuId 
			AND c.Active = 1
			AND (@TenantId IS NULL OR c.TenantId = @TenantId)
		WHERE m.Active = 1
			AND (@TenantId IS NULL OR m.TenantId = @TenantId)
		ORDER BY m.OrderBy, m.MenuName, c.OrderBy, c.CategoryName;
		
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_USER_REGISTER]
	@Name NVARCHAR(255),
	@Email NVARCHAR(255),
	@Phone NVARCHAR(50),
	@Password NVARCHAR(100),
	@TenantId BIGINT = 1,
	@AgreeToTerms BIT = 1
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		BEGIN TRANSACTION;
		
		DECLARE @UserId BIGINT;
		DECLARE @Salt NVARCHAR(100);
		DECLARE @PasswordHash NVARCHAR(100);
		DECLARE @DefaultRoleId BIGINT = 2; -- Assuming 2 is the default user role
		
		-- Check if email already exists
		IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
		BEGIN
			RAISERROR('Email address is already registered.', 16, 1);
			RETURN;
		END
		
		-- Check if phone already exists
		IF EXISTS (SELECT 1 FROM Users WHERE Phone = @Phone)
		BEGIN
			RAISERROR('Phone number is already registered.', 16, 1);
			RETURN;
		END
		
		-- Validate terms agreement
		IF @AgreeToTerms = 0
		BEGIN
			RAISERROR('You must agree to the terms and conditions.', 16, 1);
			RETURN;
		END
		
		-- Generate salt and hash password
		SET @Salt = CONVERT(NVARCHAR(50), NEWID());
		SET @PasswordHash = CONVERT(NVARCHAR(100), HASHBYTES('SHA2_256', cast(@Password as NVARCHAR(100))+ @Salt), 2);

		-- Parse name into first and last name
		DECLARE @FirstName NVARCHAR(255);
		DECLARE @LastName NVARCHAR(255);
		DECLARE @SpaceIndex INT = CHARINDEX(' ', @Name);
		
		IF @SpaceIndex > 0
		BEGIN
			SET @FirstName = LEFT(@Name, @SpaceIndex - 1);
			SET @LastName = RIGHT(@Name, LEN(@Name) - @SpaceIndex);
		END
		ELSE
		BEGIN
			SET @FirstName = @Name;
			SET @LastName = '';
		END
		
		-- Insert new user
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
			AgreeToTerms
		) VALUES (
			@FirstName,
			@LastName,
			@Email,
			@Phone,
			@PasswordHash,
			@Salt,
			@TenantId,
			1, -- Active
			0, -- EmailVerified
			0, -- PhoneVerified
			0, -- LoginAttempts
			0, -- AccountLocked
			GETUTCDATE(),
			GETUTCDATE(),
			@AgreeToTerms
		);
		
		-- Get the new user ID
		SET @UserId = SCOPE_IDENTITY();
		
		-- Assign default role to user
		INSERT INTO UserRoles (UserId, RoleId, CreatedAt)
		VALUES (@UserId, @DefaultRoleId, GETUTCDATE());
		
		-- Return user information with role
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
			u.CreatedAt,
			ur.RoleId,
			r.RoleName,
			r.RoleDescription
		FROM Users u
		LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
		LEFT JOIN Roles r ON ur.RoleId = r.RoleId
		WHERE u.UserId = @UserId;
		
		COMMIT TRANSACTION;
		
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
			
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_USER_LOGIN]
	@EmailOrPhone NVARCHAR(100),
	@Password NVARCHAR(100),
	@RememberMe BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		DECLARE @UserId BIGINT = NULL;
		DECLARE @IsActive BIT = 0;
		DECLARE @PasswordHash NVARCHAR(255);
		DECLARE @Salt NVARCHAR(100);
		DECLARE @LoginAttempts INT = 0;
		DECLARE @AccountLocked BIT = 0;
		DECLARE @LastLoginAttempt DATETIME;
		
		-- Find user by email or phone
		SELECT 
			@UserId = UserId,
			@IsActive = Active,
			@PasswordHash = PasswordHash,
			@Salt = Salt,
			@LoginAttempts = LoginAttempts,
			@AccountLocked = AccountLocked,
			@LastLoginAttempt = LastLoginAttempt
		FROM Users 
		WHERE (Email = @EmailOrPhone OR Phone = @EmailOrPhone)
			AND Active = 1;
		
		-- Check if user exists
		IF @UserId IS NULL
		BEGIN
			RAISERROR('Invalid email/phone or password.', 16, 1);
			RETURN;
		END
		
		-- Check if account is locked
		IF @AccountLocked = 1
		BEGIN
			-- Check if lock period has expired (30 minutes)
			IF DATEDIFF(MINUTE, @LastLoginAttempt, GETUTCDATE()) < 30
			BEGIN
				RAISERROR('Account is temporarily locked due to multiple failed login attempts. Please try again later.', 16, 1);
				RETURN;
			END
			ELSE
			BEGIN
				-- Unlock account
				UPDATE Users 
				SET AccountLocked = 0, LoginAttempts = 0, LastLogout=NULL
				WHERE UserId = @UserId;
				SET @AccountLocked = 0;
				SET @LoginAttempts = 0;
			END
		END
		
		-- Verify password (In real implementation, you would hash the input password with salt)
		-- For demo purposes, we'll do a simple comparison
		IF @PasswordHash != CONVERT(NVARCHAR(100), HASHBYTES('SHA2_256', cast(@Password as NVARCHAR(100))+ @Salt), 2)
		BEGIN
			-- Increment login attempts
			SET @LoginAttempts = @LoginAttempts + 1;
			
			-- Lock account after 5 failed attempts
			IF @LoginAttempts >= 5
			BEGIN
				UPDATE Users 
				SET LoginAttempts = @LoginAttempts, 
					AccountLocked = 1, 
					LastLoginAttempt = GETUTCDATE()
				WHERE UserId = @UserId;
				
				RAISERROR('Account has been locked due to multiple failed login attempts.', 16, 1);
				RETURN;
			END
			ELSE
			BEGIN
				UPDATE Users 
				SET LoginAttempts = @LoginAttempts, 
					LastLoginAttempt = GETUTCDATE()
				WHERE UserId = @UserId;
				
				RAISERROR('Invalid email/phone or password.', 16, 1);
				RETURN;
			END
		END
		
		-- Successful login - reset attempts and update last login
		UPDATE Users 
		SET LoginAttempts = 0, 
			LastLogin = GETUTCDATE(),
			LastLoginAttempt = GETUTCDATE(),
			LastLogout=NULL,
			AccountLocked = 0
		WHERE UserId = @UserId;
		
		-- Return user information
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
			@RememberMe AS RememberMe
		FROM Users u
		LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
		LEFT JOIN Roles r ON ur.RoleId = r.RoleId
		WHERE u.UserId = @UserId
			AND u.Active = 1;
		
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_GET_USER_PROFILE]
	@UserId BIGINT,
	@TenantId BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Validate user exists and is active
		IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND Active = 1)
		BEGIN
			RAISERROR('User not found or inactive.', 16, 1);
			RETURN;
		END
		
		-- Additional tenant validation if provided
		IF @TenantId IS NOT NULL
		BEGIN
			IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND TenantId = @TenantId AND Active = 1)
			BEGIN
				RAISERROR('User not found in the specified tenant.', 16, 1);
				RETURN;
			END
		END
		
		-- Get user profile information
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
			u.CreatedAt,
			u.UpdatedAt,
			u.LastLogin,
			u.LastLogout,
			u.ProfilePicture,
			u.DateOfBirth,
			u.Gender,
			u.Timezone,
			u.Language,
			u.Country,
			u.City,
			u.State,
			u.PostalCode,
			u.Bio,
			u.Website,
			u.CompanyName,
			u.JobTitle,
			u.PreferredContactMethod,
			u.NotificationSettings,
			u.PrivacySettings,
			-- User roles
			ur.RoleId,
			r.RoleName,
			r.RoleDescription,
			-- User addresses
			addr.AddressId,
			addr.AddressType,
			addr.Street,
			addr.City AS AddressCity,
			addr.State AS AddressState,
			addr.PostalCode AS AddressPostalCode,
			addr.Country AS AddressCountry,
			addr.IsDefault AS IsDefaultAddress,
			-- User preferences
			up.PreferenceKey,
			up.PreferenceValue,
			up.PreferenceType
		FROM Users u
		LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
		LEFT JOIN Roles r ON ur.RoleId = r.RoleId
		LEFT JOIN UserAddresses addr ON u.UserId = addr.UserId AND addr.Active = 1
		LEFT JOIN UserPreferences up ON u.UserId = up.UserId AND up.Active = 1
		WHERE u.UserId = @UserId 
			AND u.Active = 1
			AND (@TenantId IS NULL OR u.TenantId = @TenantId)
		ORDER BY ur.RoleId, addr.IsDefault DESC, up.PreferenceKey;
		
		-- Get user statistics (optional)
		SELECT 
			'LOGIN_COUNT' AS StatType,
			COUNT(*) AS StatValue
		FROM UserActivityLog 
		WHERE UserId = @UserId 
			AND ActivityType = 'LOGIN'
			AND CreatedAt >= DATEADD(MONTH, -12, GETUTCDATE())
		
		UNION ALL
		
		SELECT 
			'LAST_ACTIVITY' AS StatType,
			DATEDIFF(DAY, MAX(CreatedAt), GETUTCDATE()) AS StatValue
		FROM UserActivityLog 
		WHERE UserId = @UserId
		
		UNION ALL
		
		SELECT 
			'PROFILE_COMPLETION' AS StatType,
			CASE 
				WHEN ProfilePicture IS NOT NULL THEN 10 ELSE 0 END +
				CASE 
				WHEN DateOfBirth IS NOT NULL THEN 10 ELSE 0 END +
				CASE 
				WHEN Bio IS NOT NULL THEN 10 ELSE 0 END +
				CASE 
				WHEN Phone IS NOT NULL THEN 10 ELSE 0 END +
				CASE 
				WHEN EmailVerified = 1 THEN 20 ELSE 0 END +
				CASE 
				WHEN PhoneVerified = 1 THEN 20 ELSE 0 END +
				20 AS StatValue -- Base score for having name and email
		FROM Users 
		WHERE UserId = @UserId;
		
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_UPDATE_USER_PROFILE]
	@UserId BIGINT,
	@Name NVARCHAR(255) = NULL,
	@Phone NVARCHAR(50) = NULL,
	@DateOfBirth DATETIME = NULL,
	@Gender NVARCHAR(20) = NULL,
	@Bio NVARCHAR(MAX) = NULL,
	@Website NVARCHAR(255) = NULL,
	@CompanyName NVARCHAR(255) = NULL,
	@JobTitle NVARCHAR(255) = NULL,
	@Country NVARCHAR(100) = NULL,
	@City NVARCHAR(100) = NULL,
	@State NVARCHAR(100) = NULL,
	@PostalCode NVARCHAR(20) = NULL,
	@Timezone NVARCHAR(100) = NULL,
	@Language NVARCHAR(10) = NULL,
	@PreferredContactMethod NVARCHAR(50) = NULL,
	-- Address Information
	@AddressStreet NVARCHAR(255) = NULL,
	@AddressCity NVARCHAR(100) = NULL,
	@AddressState NVARCHAR(100) = NULL,
	@AddressZipCode NVARCHAR(20) = NULL,
	@AddressCountry NVARCHAR(100) = NULL,
	@AddressType NVARCHAR(50) = 'Home',
	@UpdateAddressIfExists BIT = 1
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		BEGIN TRANSACTION;
		
		-- Validate user exists and is active
		IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND Active = 1)
		BEGIN
			RAISERROR('User not found or inactive.', 16, 1);
			RETURN;
		END
		
		-- Parse name into first and last name if provided
		DECLARE @FirstName NVARCHAR(255) = NULL;
		DECLARE @LastName NVARCHAR(255) = NULL;
		
		IF @Name IS NOT NULL
		BEGIN
			DECLARE @SpaceIndex INT = CHARINDEX(' ', @Name);
			IF @SpaceIndex > 0
			BEGIN
				SET @FirstName = LEFT(@Name, @SpaceIndex - 1);
				SET @LastName = RIGHT(@Name, LEN(@Name) - @SpaceIndex);
			END
			ELSE
			BEGIN
				SET @FirstName = @Name;
				SET @LastName = '';
			END
		END
		
		-- Check if phone number already exists for another user
		IF @Phone IS NOT NULL AND EXISTS (SELECT 1 FROM Users WHERE Phone = @Phone AND UserId != @UserId)
		BEGIN
			RAISERROR('Phone number is already registered to another user.', 16, 1);
			RETURN;
		END
		
		-- Update user profile
		UPDATE Users
		SET 
			FirstName = ISNULL(@FirstName, FirstName),
			LastName = ISNULL(@LastName, LastName),
			Phone = ISNULL(@Phone, Phone),
			DateOfBirth = ISNULL(@DateOfBirth, DateOfBirth),
			Gender = ISNULL(@Gender, Gender),
			Bio = ISNULL(@Bio, Bio),
			Website = ISNULL(@Website, Website),
			CompanyName = ISNULL(@CompanyName, CompanyName),
			JobTitle = ISNULL(@JobTitle, JobTitle),
			Country = ISNULL(@Country, Country),
			City = ISNULL(@City, City),
			State = ISNULL(@State, State),
			PostalCode = ISNULL(@PostalCode, PostalCode),
			Timezone = ISNULL(@Timezone, Timezone),
			Language = ISNULL(@Language, Language),
			PreferredContactMethod = ISNULL(@PreferredContactMethod, PreferredContactMethod),
			UpdatedAt = GETUTCDATE()
		WHERE UserId = @UserId;
		
		-- Handle address update/creation if address information is provided
		IF @AddressStreet IS NOT NULL OR @AddressCity IS NOT NULL OR @AddressState IS NOT NULL 
			OR @AddressZipCode IS NOT NULL OR @AddressCountry IS NOT NULL
		BEGIN
			DECLARE @ExistingAddressId BIGINT = NULL;
			
			-- Check if user has an existing address of the specified type
			SELECT @ExistingAddressId = AddressId 
			FROM UserAddresses 
			WHERE UserId = @UserId 
				AND AddressType = @AddressType 
				AND Active = 1;
			
			IF @ExistingAddressId IS NOT NULL AND @UpdateAddressIfExists = 1
			BEGIN
				-- Update existing address
				UPDATE UserAddresses
				SET 
					Street = ISNULL(@AddressStreet, Street),
					City = ISNULL(@AddressCity, City),
					State = ISNULL(@AddressState, State),
					PostalCode = ISNULL(@AddressZipCode, PostalCode),
					Country = ISNULL(@AddressCountry, Country),
					UpdatedAt = GETUTCDATE()
				WHERE AddressId = @ExistingAddressId;
			END
			ELSE IF @ExistingAddressId IS NULL
			BEGIN
				-- Create new address
				INSERT INTO UserAddresses (
					UserId,
					AddressType,
					Street,
					City,
					State,
					PostalCode,
					Country,
					IsDefault,
					Active,
					CreatedAt,
					UpdatedAt
				) VALUES (
					@UserId,
					@AddressType,
					@AddressStreet,
					@AddressCity,
					@AddressState,
					@AddressZipCode,
					@AddressCountry,
					1, -- Set as default if it's the first address
					1,
					GETUTCDATE(),
					GETUTCDATE()
				);
			END
		END
		
		-- Log the profile update activity
		INSERT INTO UserActivityLog (
			UserId,
			ActivityType,
			ActivityDescription,
			CreatedAt
		) VALUES (
			@UserId,
			'PROFILE_UPDATE',
			'User profile updated',
			GETUTCDATE()
		);
		
		-- Return success status
		SELECT 
			@UserId AS UserId,
			'Profile updated successfully' AS Message,
			GETUTCDATE() AS UpdatedAt;
		
		COMMIT TRANSACTION;
		
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
			
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_RESET_PASSWORD]
	@UserId BIGINT,
	@NewPassword NVARCHAR(100),
	@IpAddress NVARCHAR(45) = NULL,
	@UserAgent NVARCHAR(500) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @TokenExpiry DATETIME = NULL;
		DECLARE @TokenUsed BIT = 0;
		DECLARE @Email NVARCHAR(255) = NULL;
		DECLARE @Salt NVARCHAR(50) = NULL;
		DECLARE @HashedPassword NVARCHAR(255) = NULL;
		DECLARE @CurrentTime DATETIME = GETUTCDATE();
		
		-- Validate reset token and get user information
		SELECT 
			@UserId = UserId
		FROM Users 
		WHERE Active = 1 AND UserId=@UserId;
		
		-- Check if token exists
		IF @UserId IS NULL
		BEGIN
			RAISERROR('Invalid or expired reset USER.', 16, 1);
			RETURN;
		END
		
		-- Generate a new salt and hash the password
		SET @Salt = CONVERT(NVARCHAR(50), NEWID());						  
		SET @HashedPassword = CONVERT(NVARCHAR(100), HASHBYTES('SHA2_256', cast(@NewPassword as NVARCHAR(100))+ @Salt), 2);
		
		-- Update user password	
		UPDATE Users
		SET 
			PasswordHash = @HashedPassword,
			Salt = @Salt,
			PasswordChangedAt = @CurrentTime,
			LastPasswordReset = @CurrentTime,
			UpdatedAt = @CurrentTime,
			-- Reset failed login attempts since password was successfully reset
			LoginAttempts = 0,
			AccountLocked = 0
		WHERE UserId = @UserId;		
		
		-- Log the password reset activity
		INSERT INTO UserActivityLog (
			UserId,
			ActivityType,
			ActivityDescription,
			IpAddress,
			UserAgent,
			CreatedAt
		) VALUES (
			@UserId,
			'PASSWORD_RESET',
			'Password reset successfully using reset token',
			@IpAddress,
			@UserAgent,
			@CurrentTime
		);
		
		-- Optional: Send notification email (placeholder for email service integration)
		-- This would typically trigger an email notification about successful password reset
		INSERT INTO UserNotifications (
			UserId,
			NotificationType,
			Title,
			Message,
			IsRead,
			CreatedAt
		) VALUES (
			@UserId,
			'SECURITY_ALERT',
			'Password Reset Successful',
			'Your password has been successfully reset. If you did not perform this action, please contact support immediately.',
			0,
			@CurrentTime
		);
		
		-- Return success information
		SELECT 
			@UserId AS UserId,
			@Email AS Email,
			'Password reset successfully' AS Message,
			@CurrentTime AS ResetDate;
		
		COMMIT TRANSACTION;
		
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
			
		DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
		DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
		DECLARE @ErrorState INT = ERROR_STATE();
		
		-- Log the failed password reset attempt
		IF @UserId IS NOT NULL
		BEGIN
			INSERT INTO UserActivityLog (
				UserId,
				ActivityType,
				ActivityDescription,
				IpAddress,
				UserAgent,
				CreatedAt
			) VALUES (
				@UserId,
				'PASSWORD_RESET_FAILED',
				'Failed password reset attempt: ' + @ErrorMessage,
				@IpAddress,
				@UserAgent,
				GETUTCDATE()
			);
		END
		
		RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[SP_SEARCH_PRODUCTS]
	@TenantId BIGINT,
	@Page INT = 1,
	@Limit INT = 10,
	@Offset INT = 0,
	@Search NVARCHAR(255) = '',
	@Category INT = NULL,
	@MinPrice DECIMAL(18,2) = NULL,
	@MaxPrice DECIMAL(18,2) = NULL,
	@Rating INT = NULL,
	@InStock BIT = NULL,
	@BestSeller BIT = NULL,
	@HasOffer BIT = NULL,
	@SortBy NVARCHAR(50) = 'created',
	@SortOrder NVARCHAR(10) = 'desc'
AS
BEGIN
	SET NOCOUNT ON;
	
	-- Declare variables
	DECLARE @SQL NVARCHAR(MAX);
	DECLARE @WhereClause NVARCHAR(MAX) = '';
	DECLARE @OrderByClause NVARCHAR(MAX) = '';
	DECLARE @TotalCount INT = 0;
	
	-- Build WHERE clause dynamically
	SET @WhereClause = 'WHERE p.TenantId = ' + CAST(@TenantId AS NVARCHAR) + ' ';
	
	-- Search filter
	IF @Search IS NOT NULL AND @Search != ''
	BEGIN
		SET @WhereClause = @WhereClause + 
			'AND (p.ProductName LIKE ''%' + @Search + '%'' 
			 OR p.ProductDescription LIKE ''%' + @Search + '%'' 
			 OR p.ProductCode LIKE ''%' + @Search + '%'') ';
	END
	
	-- Category filter
	IF @Category IS NOT NULL
	BEGIN
		SET @WhereClause = @WhereClause + 'AND p.Category = ' + CAST(@Category AS NVARCHAR) + ' ';
	END
	
	-- Price range filter
	IF @MinPrice IS NOT NULL
	BEGIN
		SET @WhereClause = @WhereClause + 'AND p.Price >= ' + CAST(@MinPrice AS NVARCHAR) + ' ';
	END
	
	IF @MaxPrice IS NOT NULL
	BEGIN
		SET @WhereClause = @WhereClause + 'AND p.Price <= ' + CAST(@MaxPrice AS NVARCHAR) + ' ';
	END
	
	-- Rating filter
	IF @Rating IS NOT NULL
	BEGIN
		SET @WhereClause = @WhereClause + 'AND p.Rating >= ' + CAST(@Rating AS NVARCHAR) + ' ';
	END
	
	-- Stock filter
	IF @InStock IS NOT NULL
	BEGIN
		IF @InStock = 1
			SET @WhereClause = @WhereClause + 'AND p.Quantity > 0 ';
		ELSE
			SET @WhereClause = @WhereClause + 'AND p.Quantity = 0 ';
	END
	
	-- Best seller filter
	IF @BestSeller IS NOT NULL AND @BestSeller = 1
	BEGIN
		SET @WhereClause = @WhereClause + 'AND p.BestSeller = 1 ';
	END
	
	-- Has offer filter
	IF @HasOffer IS NOT NULL AND @HasOffer = 1
	BEGIN
		SET @WhereClause = @WhereClause + 'AND (p.Offer IS NOT NULL AND p.Offer != '''') ';
	END
	
	-- Active products only
	SET @WhereClause = @WhereClause + 'AND p.Active = 1 ';
	
	-- Build ORDER BY clause
	SET @OrderByClause = 'ORDER BY ';
	
	IF @SortBy = 'productName'
		SET @OrderByClause = @OrderByClause + 'p.ProductName ';
	ELSE IF @SortBy = 'price'
		SET @OrderByClause = @OrderByClause + 'p.Price ';
	ELSE IF @SortBy = 'rating'
		SET @OrderByClause = @OrderByClause + 'p.Rating ';
	ELSE IF @SortBy = 'userBuyCount'
		SET @OrderByClause = @OrderByClause + 'p.UserBuyCount ';
	ELSE
		SET @OrderByClause = @OrderByClause + 'p.Created ';
	
	IF @SortOrder = 'asc'
		SET @OrderByClause = @OrderByClause + 'ASC ';
	ELSE
		SET @OrderByClause = @OrderByClause + 'DESC ';
	
	-- Get total count first
	SET @SQL = '
	SELECT COUNT(*) as TotalCount
	FROM Products p 
	' + @WhereClause;
	
	-- Create temp table for count
	CREATE TABLE #TempCount (TotalCount INT);
	INSERT INTO #TempCount
	EXEC sp_executesql @SQL;
	
	SELECT @TotalCount = TotalCount FROM #TempCount;
	DROP TABLE #TempCount;
	
	-- Get paginated results
	SET @SQL = '
	SELECT 
		p.ProductId,
		p.TenantId,
		p.ProductName,
		p.ProductDescription,
		p.ProductCode,
		p.FullDescription,
		p.Specification,
		p.Story,
		p.PackQuantity,
		p.Quantity,
		p.Total,
		p.Price,
		p.Category,
		p.Rating,
		p.Active,
		p.Trending,
		p.UserBuyCount,
		p.[Return],
		p.Created,
		p.Modified,
		CASE WHEN p.Quantity > 0 THEN 1 ELSE 0 END as InStock,
		p.BestSeller,
		p.DeliveryDate,
		p.Offer,
		p.OrderBy,
		p.UserId,
		p.Overview,
		p.LongDescription
	FROM Products p 
	' + @WhereClause + @OrderByClause + '
	OFFSET ' + CAST(@Offset AS VARCHAR) + ' ROWS
	FETCH NEXT ' + CAST(@Limit AS VARCHAR) + ' ROWS ONLY';
	
	-- Execute main query
	EXEC sp_executesql @SQL;
	
	-- Return total count as second result set
	SELECT @TotalCount as TotalCount;
	
END
GO

CREATE PROCEDURE [dbo].[SP_GET_PRODUCT_BY_ID]
(
	@ProductId BIGINT
)
AS
BEGIN
		SELECT 
			p.ProductId,
			p.TenantId,
			p.ProductName,
			p.ProductDescription,
			p.ProductCode,
			p.FullDescription,
			p.Specification,
			p.Story,
			p.PackQuantity,
			p.Quantity,
			p.Total,
			p.Price,
			p.Category,
			p.Rating,
			p.Active,
			p.Trending,
			p.UserBuyCount,
			p.[Return],
			p.Created,
			p.Modified,
			CASE WHEN p.Quantity > 0 THEN 1 ELSE 0 END as InStock,
			p.BestSeller,
			p.DeliveryDate,
			p.Offer,
			p.OrderBy,
			p.UserId,
			p.Overview,
			p.LongDescription
		FROM Products p WITH (NOLOCK)
		WHERE p.ProductId = @ProductId
			AND p.Active = 1;

		-- Get product images
		SELECT 
			i.ImageId,
			i.ImageName as Poster,
			i.Main as [Main],
			i.Active,
			i.OrderBy
		FROM ProductImages i WITH (NOLOCK)
		WHERE i.ProductId = @ProductId
			AND i.Active = 1
		ORDER BY i.OrderBy;
	END
	GO

CREATE PROCEDURE [dbo].[SP_ADD_PRODUCT]
			@TenantId BIGINT,
			@ProductName NVARCHAR(255),
			@ProductDescription NVARCHAR(500),
			@ProductCode NVARCHAR(100),
			@FullDescription NVARCHAR(MAX),
			@Specification NVARCHAR(MAX),
			@Story NVARCHAR(MAX),
			@PackQuantity INT,
			@Quantity INT,
			@Total INT,
			@Price DECIMAL(18,2),
			@Category INT,
			@Rating INT,
			@Active BIT,
			@Trending INT,
			@UserBuyCount INT,
			@Return INT,
			@BestSeller BIT,
			@DeliveryDate INT,
			@Offer NVARCHAR(100),
			@OrderBy INT,
			@UserId BIGINT,
			@CreatedBy BIGINT
		AS
		BEGIN
			SET NOCOUNT ON;
			
			BEGIN TRY
				BEGIN TRANSACTION;
				
				-- Check if product code already exists for this tenant
				IF EXISTS (SELECT 1 FROM Products WHERE TenantId = @TenantId AND ProductCode = @ProductCode)
				BEGIN
					RAISERROR('Product code already exists for this tenant.', 16, 1);
					RETURN;
				END
				
				-- Insert new product
				INSERT INTO Products (
					TenantId,
					ProductName,
					ProductDescription,
					ProductCode,
					FullDescription,
					Specification,
					Story,
					PackQuantity,
					Quantity,
					Total,
					Price,
					Category,
					Rating,
					Active,
					Trending,
					UserBuyCount,
					[Return],
					BestSeller,
					DeliveryDate,
					Offer,
					OrderBy,
					UserId,
					Created,
					Modified,
					CreatedBy,
					ModifiedBy
				)
				VALUES (
					@TenantId,
					@ProductName,
					@ProductDescription,
					@ProductCode,
					@FullDescription,
					@Specification,
					@Story,
					@PackQuantity,
					@Quantity,
					@Total,
					@Price,
					@Category,
					@Rating,
					@Active,
					@Trending,
					@UserBuyCount,
					@Return,
					@BestSeller,
					@DeliveryDate,
					@Offer,
					@OrderBy,
					@UserId,
					GETUTCDATE(),
					GETUTCDATE(),
					@CreatedBy,
					@CreatedBy
				);
				
				-- Get the new product ID
				DECLARE @ProductId BIGINT = SCOPE_IDENTITY();
				
				-- Return the new product ID
				SELECT @ProductId AS ProductId;
				
				COMMIT TRANSACTION;
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
					
				DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
				DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
				DECLARE @ErrorState INT = ERROR_STATE();
				
				RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
			END CATCH
		END
	GO

		CREATE PROCEDURE [dbo].[SP_UPDATE_PRODUCT]
			@ProductId BIGINT,
			@TenantId BIGINT,
			@ProductName NVARCHAR(255),
			@ProductDescription NVARCHAR(500),
			@ProductCode NVARCHAR(100),
			@FullDescription NVARCHAR(MAX),
			@Specification NVARCHAR(MAX),
			@Story NVARCHAR(MAX),
			@PackQuantity INT,
			@Quantity INT,
			@Total INT,
			@Price DECIMAL(18,2),
			@Category INT,
			@Rating INT,
			@Active BIT,
			@Trending INT,
			@UserBuyCount INT,
			@Return INT,
			@BestSeller BIT,
			@DeliveryDate INT,
			@Offer NVARCHAR(100),
			@OrderBy INT,
			@UserId BIGINT,
			@ModifiedBy BIGINT
		AS
		BEGIN
			SET NOCOUNT ON;
			
			BEGIN TRY
				BEGIN TRANSACTION;
				
				-- Check if product exists and belongs to the tenant
				IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductId = @ProductId AND TenantId = @TenantId)
				BEGIN
					RAISERROR('Product not found or does not belong to this tenant.', 16, 1);
					RETURN;
				END
				
				-- Check if product code already exists for another product of this tenant
				IF EXISTS (
					SELECT 1 
					FROM Products 
					WHERE TenantId = @TenantId 
						AND ProductCode = @ProductCode 
						AND ProductId != @ProductId
				)
				BEGIN
					RAISERROR('Product code already exists for another product.', 16, 1);
					RETURN;
				END
				
				-- Update product
				UPDATE Products
				SET
					ProductName = @ProductName,
					ProductDescription = @ProductDescription,
					ProductCode = @ProductCode,
					FullDescription = @FullDescription,
					Specification = @Specification,
					Story = @Story,
					PackQuantity = @PackQuantity,
					Quantity = @Quantity,
					Total = @Total,
					Price = @Price,
					Category = @Category,
					Rating = @Rating,
					Active = @Active,
					Trending = @Trending,
					UserBuyCount = @UserBuyCount,
					[Return] = @Return,
					BestSeller = @BestSeller,
					DeliveryDate = @DeliveryDate,
					Offer = @Offer,
					OrderBy = @OrderBy,
					UserId = @UserId,
					Modified = GETUTCDATE(),
					ModifiedBy = @ModifiedBy
				WHERE ProductId = @ProductId
					AND TenantId = @TenantId;
				
				-- Return success
				SELECT @ProductId AS ProductId;
				
				COMMIT TRANSACTION;
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
					
				DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
				DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
				DECLARE @ErrorState INT = ERROR_STATE();
				
				RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
			END CATCH
		END
	GO


		CREATE PROCEDURE [dbo].[SP_DELETE_PRODUCT]
			@ProductId BIGINT,
			@TenantId BIGINT,
			@UserId BIGINT
		AS
		BEGIN
			SET NOCOUNT ON;
			
			BEGIN TRY
				BEGIN TRANSACTION;
				
				-- Check if product exists and belongs to the tenant
				IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductId = @ProductId AND TenantId = @TenantId)
				BEGIN
					RAISERROR('Product not found or does not belong to this tenant.', 16, 1);
					RETURN;
				END
				
				-- Check if product is referenced in any orders or carts
				IF EXISTS (
					SELECT 1 
					FROM CartItems 
					WHERE ProductId = @ProductId AND Active = 1
					   OR EXISTS (SELECT 1 FROM OrderItems WHERE ProductId = @ProductId AND Active = 1)
				)
				BEGIN
					-- Soft delete - just mark as inactive
					UPDATE Products
					SET 
						Active = 0,
						Modified = GETUTCDATE(),
						ModifiedBy = @UserId
					WHERE ProductId = @ProductId
						AND TenantId = @TenantId;
				END
				ELSE
				BEGIN
					-- Hard delete - first delete related records
					DELETE FROM ProductImages WHERE ProductId = @ProductId;
					DELETE FROM ProductReviews WHERE ProductId = @ProductId;
					DELETE FROM ProductWishList WHERE ProductId = @ProductId;
					
					-- Then delete the product
					DELETE FROM Products 
					WHERE ProductId = @ProductId 
						AND TenantId = @TenantId;
				END
				
				-- Return success
				SELECT @ProductId AS ProductId;
				
				COMMIT TRANSACTION;
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
					
				DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
				DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
				DECLARE @ErrorState INT = ERROR_STATE();
				
				RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
			END CATCH
		END
		GO


				CREATE PROCEDURE [dbo].[SP_ADD_ITEM_TO_CART]
			@UserId BIGINT,
			@ProductId BIGINT,
			@Quantity INT,
			@TenantId BIGINT = NULL,
			@SessionId NVARCHAR(255) = NULL,
			@IpAddress NVARCHAR(45) = NULL,
			@UserAgent NVARCHAR(500) = NULL
		AS
		BEGIN
			SET NOCOUNT ON;
			
			BEGIN TRY
				BEGIN TRANSACTION;
				
				DECLARE @ExistingCartId BIGINT = NULL;
				DECLARE @ExistingQuantity INT = 0;
				DECLARE @ProductPrice DECIMAL(18,2) = 0;
				DECLARE @ProductName NVARCHAR(255) = '';
				DECLARE @AvailableStock INT = 0;
				DECLARE @ProductActive BIT = 0;
				DECLARE @CurrentTime DATETIME = GETUTCDATE();
				DECLARE @NewQuantity INT = 0;
				
				-- Validate user exists and is active
				IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND Active = 1)
				BEGIN
					RAISERROR('User not found or inactive.', 16, 1);
					RETURN;
				END
				
				-- Validate product exists, is active, and get product details
				SELECT 
					@ProductPrice = Price,
					@ProductName = ProductName,
					@AvailableStock = Quantity,
					@ProductActive = Active
				FROM Products 
				WHERE ProductId = @ProductId
					AND (@TenantId IS NULL OR TenantId = @TenantId);
				
				IF @ProductActive IS NULL OR @ProductActive = 0
				BEGIN
					RAISERROR('Product not found or inactive.', 16, 1);
					RETURN;
				END
				
				-- Validate quantity is positive
				IF @Quantity <= 0
				BEGIN
					RAISERROR('Quantity must be greater than 0.', 16, 1);
					RETURN;
				END
				
				-- Check if item already exists in cart
				SELECT 
					@ExistingCartId = CartId,
					@ExistingQuantity = Quantity
				FROM CartItems 
				WHERE UserId = @UserId 
					AND ProductId = @ProductId 
					AND Active = 1
					AND (@TenantId IS NULL OR TenantId = @TenantId);
				
				-- Calculate new quantity
				SET @NewQuantity = @ExistingQuantity + @Quantity;
				
				-- Check stock availability
				IF @NewQuantity > @AvailableStock
				BEGIN
					RAISERROR('Insufficient stock. Available quantity: %d, Requested quantity: %d.', 16, 1, @AvailableStock, @NewQuantity);
					RETURN;
				END
				
				IF @ExistingCartId IS NOT NULL
				BEGIN
					-- Update existing cart item
					UPDATE CartItems
					SET 
						Quantity = @NewQuantity,
						UpdatedDate = @CurrentTime,
						SessionId = ISNULL(@SessionId, SessionId)
					WHERE CartId = @ExistingCartId;
					
					-- Log the cart update activity
					INSERT INTO UserActivityLog (
						UserId,
						ActivityType,
						ActivityDescription,
						IpAddress,
						UserAgent,
						CreatedAt
					) VALUES (
						@UserId,
						'CART_UPDATE',
						'Updated cart item: ' + @ProductName + ' (Quantity: ' + CAST(@NewQuantity AS VARCHAR(10)) + ')',
						@IpAddress,
						@UserAgent,
						@CurrentTime
					);
					
					-- Return updated cart item info
					SELECT 
						@ExistingCartId AS CartId,
						@UserId AS UserId,
						@ProductId AS ProductId,
						@ProductName AS ProductName,
						@NewQuantity AS Quantity,
						@ProductPrice AS Price,
						(@NewQuantity * @ProductPrice) AS ItemTotal,
						'Product quantity updated in cart' AS Message,
						@CurrentTime AS UpdatedDate;
				END
				ELSE
				BEGIN
					-- Insert new cart item
					INSERT INTO CartItems (
						UserId,
						ProductId,
						Quantity,
						TenantId,
						SessionId,
						Active,
						AddedDate,
						UpdatedDate
					) VALUES (
						@UserId,
						@ProductId,
						@Quantity,
						@TenantId,
						@SessionId,
						1,
						@CurrentTime,
						@CurrentTime
					);
					
					SET @ExistingCartId = SCOPE_IDENTITY();
					
					-- Log the cart addition activity
					INSERT INTO UserActivityLog (
						UserId,
						ActivityType,
						ActivityDescription,
						IpAddress,
						UserAgent,
						CreatedAt
					) VALUES (
						@UserId,
						'CART_ADD',
						'Added to cart: ' + @ProductName + ' (Quantity: ' + CAST(@Quantity AS VARCHAR(10)) + ')',
						@IpAddress,
						@UserAgent,
						@CurrentTime
					);
					
					-- Return new cart item info
					SELECT 
						@ExistingCartId AS CartId,
						@UserId AS UserId,
						@ProductId AS ProductId,
						@ProductName AS ProductName,
						@Quantity AS Quantity,
						@ProductPrice AS Price,
						(@Quantity * @ProductPrice) AS ItemTotal,
						'Product added to cart successfully' AS Message,
						@CurrentTime AS AddedDate;
				END
				
				-- Optional: Clean up old inactive cart items for this user (housekeeping)
				DELETE FROM CartItems 
				WHERE UserId = @UserId 
					AND Active = 0 
					AND UpdatedDate < DATEADD(DAY, -30, @CurrentTime);
				
				-- Get updated cart summary
				SELECT 
					COUNT(*) AS TotalUniqueItems,
					SUM(p.Quantity) AS TotalQuantity,
					SUM(ci.Quantity * p.Price) AS TotalAmount
				FROM CartItems ci
				INNER JOIN Products p ON ci.ProductId = p.ProductId
				WHERE ci.UserId = @UserId 
					AND ci.Active = 1
					AND p.Active = 1
					AND (@TenantId IS NULL OR ci.TenantId = @TenantId);
				
				COMMIT TRANSACTION;
				
			END TRY
			BEGIN CATCH
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
					
				DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
				DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
				DECLARE @ErrorState INT = ERROR_STATE();
				
				-- Log the failed cart operation
				IF @UserId IS NOT NULL
				BEGIN
					INSERT INTO UserActivityLog (
						UserId,
						ActivityType,
						ActivityDescription,
						IpAddress,
						UserAgent,
						CreatedAt
					) VALUES (
						@UserId,
						'CART_ADD_FAILED',
						'Failed to add item to cart: ' + @ErrorMessage,
						@IpAddress,
						@UserAgent,
						GETUTCDATE()
					);
				END
				
				RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
			END CATCH
		END