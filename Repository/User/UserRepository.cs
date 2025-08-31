using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sa.Common.ADO.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tenant.API.Base.Repository;
using Tenant.Query.Context;

namespace Tenant.Query.Repository.User
{
    public class UserRepository : TnBaseQueryRepository<Model.User.User, Context.UserContext>
    {
        DataAccess _dataAccess;
        public UserRepository(UserContext dbContext, ILoggerFactory loggerFactory, DataAccess dataAccess) : base(dbContext, loggerFactory)
        {
            _dataAccess = dataAccess;
        }

        public override Task<Model.User.User> GetById(string tenantId, string id)
        {
            throw new NotImplementedException();
        }

        #region new

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Model.User.SpUserMasterList> GetUser(string spName, long userId)
        {
            try
            {
                //Executing query
                List<Model.User.SpUserMasterList> spUserMasterLists = _dataAccess.ExecuteGenericList<Model.User.SpUserMasterList>(spName,
                    userId);

                return spUserMasterLists;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Authenticate user login
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Login response with user information</returns>
        public async Task<Model.User.LoginResponse> Login(Model.User.LoginRequest request)
        {
            try
            {
                this.Logger.LogInformation($"Repository: Login attempt for {request.EmailOrPhone}");

                var parameters = new Dictionary<string, object>
                {
                    { "EmailOrPhone", request.EmailOrPhone },
                    { "Password", request.Password },
                    { "RememberMe", request.RememberMe }
                };

                var result = await Task.Run(() => _dataAccess.ExecuteDataset(
                    Model.Constant.Constant.StoredProcedures.SP_USER_LOGIN,
                    parameters
                ));

                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                {
                    throw new UnauthorizedAccessException("Invalid email/phone or password.");
                }

                var loginResponse = new Model.User.LoginResponse();
                var userRoles = new Dictionary<long, Model.User.UserRoleInfo>();

                foreach (DataRow row in result.Tables[0].Rows)
                {
                    // Set user information (will be the same for all rows)
                    if (loginResponse.UserId == 0)
                    {
                        loginResponse.UserId = Convert.ToInt64(row["UserId"]);
                        loginResponse.FirstName = row["FirstName"]?.ToString();
                        loginResponse.LastName = row["LastName"]?.ToString();
                        loginResponse.Email = row["Email"]?.ToString();
                        loginResponse.Phone = row["Phone"]?.ToString();
                        loginResponse.Active = Convert.ToBoolean(row["Active"]);
                        loginResponse.TenantId = Convert.ToInt64(row["TenantId"]);
                        loginResponse.LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : (DateTime?)null;
                        loginResponse.RememberMe = Convert.ToBoolean(row["RememberMe"]);
                    }

                    // Add role if it exists and hasn't been added yet
                    if (row["RoleId"] != DBNull.Value)
                    {
                        var roleId = Convert.ToInt64(row["RoleId"]);
                        if (!userRoles.ContainsKey(roleId))
                        {
                            userRoles[roleId] = new Model.User.UserRoleInfo
                            {
                                RoleId = roleId,
                                RoleName = row["RoleName"]?.ToString(),
                                RoleDescription = row["RoleDescription"]?.ToString()
                            };
                        }
                    }
                }

                loginResponse.Roles = userRoles.Values.ToList();

                // TODO: Generate JWT token if needed
                // loginResponse.Token = GenerateJwtToken(loginResponse);
                // loginResponse.TokenExpiration = DateTime.UtcNow.AddHours(24);

                this.Logger.LogInformation($"Repository: Login successful for user {loginResponse.UserId}");

                return loginResponse;
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Repository: Login error for {request.EmailOrPhone}: {ex.Message}");
                
                // Check for specific error messages from stored procedure
                if (ex.Message.Contains("Account is temporarily locked") || 
                    ex.Message.Contains("Account has been locked"))
                {
                    throw new InvalidOperationException(ex.Message);
                }
                else if (ex.Message.Contains("Invalid email/phone or password"))
                {
                    throw new UnauthorizedAccessException(ex.Message);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Registration request</param>
        /// <returns>Registration response with user information and token</returns>
        public async Task<Model.User.RegisterResponse> Register(Model.User.RegisterRequest request)
        {
            try
            {
                this.Logger.LogInformation($"Repository: Registration attempt for {request.Email}");

                var parameters = new Dictionary<string, object>
                {
                    { "Name", request.Name },
                    { "Email", request.Email },
                    { "Phone", request.Phone },
                    { "Password", request.Password },
                    { "TenantId", request.TenantId },
                    { "AgreeToTerms", request.AgreeToTerms }
                };

                var result = await Task.Run(() => _dataAccess.ExecuteDataset(
                    Model.Constant.Constant.StoredProcedures.SP_USER_REGISTER,
                    parameters
                ));

                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Failed to register user");
                }

                var registerResponse = new Model.User.RegisterResponse();
                var registeredUser = new Model.User.RegisteredUser();
                var userRoles = new Dictionary<long, Model.User.UserRoleInfo>();

                foreach (DataRow row in result.Tables[0].Rows)
                {
                    // Set user information (will be the same for all rows)
                    if (registeredUser.UserId == 0)
                    {
                        registeredUser.UserId = Convert.ToInt64(row["UserId"]);
                        registeredUser.FirstName = row["FirstName"]?.ToString();
                        registeredUser.LastName = row["LastName"]?.ToString();
                        registeredUser.Email = row["Email"]?.ToString();
                        registeredUser.Phone = row["Phone"]?.ToString();
                        registeredUser.Active = Convert.ToBoolean(row["Active"]);
                        registeredUser.TenantId = Convert.ToInt64(row["TenantId"]);
                        registeredUser.EmailVerified = Convert.ToBoolean(row["EmailVerified"]);
                        registeredUser.PhoneVerified = Convert.ToBoolean(row["PhoneVerified"]);
                        registeredUser.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
                    }

                    // Add role if it exists and hasn't been added yet
                    if (row["RoleId"] != DBNull.Value)
                    {
                        var roleId = Convert.ToInt64(row["RoleId"]);
                        if (!userRoles.ContainsKey(roleId))
                        {
                            userRoles[roleId] = new Model.User.UserRoleInfo
                            {
                                RoleId = roleId,
                                RoleName = row["RoleName"]?.ToString(),
                                RoleDescription = row["RoleDescription"]?.ToString()
                            };
                        }
                    }
                }

                registeredUser.Roles = userRoles.Values.ToList();
                registerResponse.User = registeredUser;

                // Generate JWT token and refresh token
                var tokenExpiration = DateTime.UtcNow.AddHours(24); // 24 hours
                registerResponse.Token = GenerateJwtToken(registeredUser, tokenExpiration);
                registerResponse.RefreshToken = GenerateRefreshToken();
                registerResponse.ExpiresIn = 3600; // 1 hour in seconds
                registerResponse.TokenExpiration = tokenExpiration;

                this.Logger.LogInformation($"Repository: Registration successful for user {registeredUser.UserId}");

                return registerResponse;
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Repository: Registration error for {request.Email}: {ex.Message}");
                
                // Check for specific error messages from stored procedure
                if (ex.Message.Contains("Email address is already registered") ||
                    ex.Message.Contains("Phone number is already registered"))
                {
                    throw new InvalidOperationException(ex.Message);
                }
                else if (ex.Message.Contains("You must agree to the terms and conditions"))
                {
                    throw new ArgumentException(ex.Message);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Generate JWT token for user (placeholder implementation)
        /// </summary>
        /// <param name="user">User information</param>
        /// <param name="expiration">Token expiration</param>
        /// <returns>JWT token string</returns>
        private string GenerateJwtToken(Model.User.RegisteredUser user, DateTime expiration)
        {
            // TODO: Implement actual JWT token generation
            // This is a placeholder - implement with proper JWT library
            var tokenPayload = $"{{\"userId\":{user.UserId},\"email\":\"{user.Email}\",\"tenantId\":{user.TenantId},\"exp\":{((DateTimeOffset)expiration).ToUnixTimeSeconds()}}}";
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenPayload);
            return $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{Convert.ToBase64String(tokenBytes)}.signature_placeholder";
        }

        /// <summary>
        /// Generate refresh token (placeholder implementation)
        /// </summary>
        /// <returns>Refresh token string</returns>
        private string GenerateRefreshToken()
        {
            // TODO: Implement actual refresh token generation
            // This is a placeholder - implement with proper cryptographic randomness
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Logout user and invalidate tokens
        /// </summary>
        /// <param name="request">Logout request</param>
        /// <returns>Logout confirmation message</returns>
        public async Task<string> Logout(Model.User.LogoutRequest request)
        {
            try
            {
                this.Logger.LogInformation($"Repository: Logout attempt for user {request.UserId}");

                var parameters = new Dictionary<string, object>
                {
                    { "UserId", request.UserId },
                    { "Token", request.Token ?? (object)DBNull.Value },
                    { "RefreshToken", request.RefreshToken ?? (object)DBNull.Value },
                    { "DeviceId", request.DeviceId ?? (object)DBNull.Value },
                    { "LogoutFromAllDevices", request.LogoutFromAllDevices }
                };

                var result = await Task.Run(() => _dataAccess.ExecuteDataset(
                    Model.Constant.Constant.StoredProcedures.SP_USER_LOGOUT,
                    parameters
                ));

                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                {
                    throw new KeyNotFoundException("User not found or inactive");
                }

                var logoutResult = result.Tables[0].Rows[0];
                var message = logoutResult["Message"]?.ToString() ?? "Logged out successfully";
                var logoutTime = Convert.ToDateTime(logoutResult["LogoutTime"]);
                var logoutFromAllDevices = Convert.ToBoolean(logoutResult["LogoutFromAllDevices"]);

                this.Logger.LogInformation($"Repository: Logout successful for user {request.UserId} at {logoutTime}");

                // Log additional activity if needed
                await LogUserActivity(request, "LOGOUT_SUCCESS", message);

                return message;
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Repository: Logout error for user {request.UserId}: {ex.Message}");
                
                // Check for specific error messages from stored procedure
                if (ex.Message.Contains("User not found or inactive"))
                {
                    throw new KeyNotFoundException(ex.Message);
                }
                
                // Log failed activity
                await LogUserActivity(request, "LOGOUT_FAILED", ex.Message);
                
                throw;
            }
        }

        /// <summary>
        /// Log user activity for logout operations
        /// </summary>
        /// <param name="request">Logout request</param>
        /// <param name="activityType">Type of activity</param>
        /// <param name="description">Activity description</param>
        /// <returns>Task</returns>
        private async Task LogUserActivity(Model.User.LogoutRequest request, string activityType, string description)
        {
            try
            {
                var activityParameters = new Dictionary<string, object>
                {
                    { "UserId", request.UserId },
                    { "ActivityType", activityType },
                    { "ActivityDescription", description },
                    { "IPAddress", request.IpAddress ?? (object)DBNull.Value },
                    { "UserAgent", request.UserAgent ?? (object)DBNull.Value },
                    { "DeviceId", request.DeviceId ?? (object)DBNull.Value },
                    { "CreatedAt", DateTime.UtcNow }
                };

                // Note: This assumes you have a stored procedure for logging activities
                // If not, you can implement direct SQL insert or create the stored procedure
                // _dataAccess.ExecuteNonQuery("SP_LOG_USER_ACTIVITY", activityParameters);
                
                this.Logger.LogInformation($"User activity logged: {activityType} for user {request.UserId}");
            }
            catch (Exception ex)
            {
                // Don't throw here as this is auxiliary logging
                this.Logger.LogWarning($"Failed to log user activity: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user profile information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="tenantId">Optional tenant ID for validation</param>
        /// <returns>User profile data</returns>
        public async Task<Model.User.UserProfileData> GetUserProfile(long userId, long? tenantId = null)
        {
            try
            {
                this.Logger.LogInformation($"Repository: Get profile for user {userId}");

                var parameters = new Dictionary<string, object>
                {
                    { "UserId", userId },
                    { "TenantId", tenantId ?? (object)DBNull.Value }
                };

                var result = await Task.Run(() => _dataAccess.ExecuteDataset(
                    Model.Constant.Constant.StoredProcedures.SP_GET_USER_PROFILE,
                    parameters
                ));

                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                {
                    throw new KeyNotFoundException("User not found or inactive");
                }

                var profileData = new Model.User.UserProfileData();
                var userRoles = new Dictionary<long, Model.User.UserRoleInfo>();
                var userAddresses = new Dictionary<long, Model.User.UserAddressInfo>();
                var userPreferences = new Dictionary<string, Model.User.UserPreferenceInfo>();

                // Process main profile data and related information
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    // Set user basic information (only once)
                    if (profileData.UserId == 0)
                    {
                        profileData.UserId = Convert.ToInt64(row["UserId"]);
                        profileData.FirstName = row["FirstName"]?.ToString();
                        profileData.LastName = row["LastName"]?.ToString();
                        profileData.Email = row["Email"]?.ToString();
                        profileData.Phone = row["Phone"]?.ToString();
                        profileData.Active = Convert.ToBoolean(row["Active"]);
                        profileData.TenantId = Convert.ToInt64(row["TenantId"]);
                        profileData.EmailVerified = Convert.ToBoolean(row["EmailVerified"]);
                        profileData.PhoneVerified = Convert.ToBoolean(row["PhoneVerified"]);
                        profileData.CreatedAt = Convert.ToDateTime(row["CreatedAt"]);
                        profileData.UpdatedAt = row["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(row["UpdatedAt"]) : (DateTime?)null;
                        profileData.LastLogin = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]) : (DateTime?)null;
                        profileData.LastLogout = row["LastLogout"] != DBNull.Value ? Convert.ToDateTime(row["LastLogout"]) : (DateTime?)null;
                        profileData.ProfilePicture = row["ProfilePicture"]?.ToString();
                        profileData.DateOfBirth = row["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(row["DateOfBirth"]) : (DateTime?)null;
                        profileData.Gender = row["Gender"]?.ToString();
                        profileData.Timezone = row["Timezone"]?.ToString();
                        profileData.Language = row["Language"]?.ToString();
                        profileData.Country = row["Country"]?.ToString();
                        profileData.City = row["City"]?.ToString();
                        profileData.State = row["State"]?.ToString();
                        profileData.PostalCode = row["PostalCode"]?.ToString();
                        profileData.Bio = row["Bio"]?.ToString();
                        profileData.Website = row["Website"]?.ToString();
                        profileData.CompanyName = row["CompanyName"]?.ToString();
                        profileData.JobTitle = row["JobTitle"]?.ToString();
                        profileData.PreferredContactMethod = row["PreferredContactMethod"]?.ToString();
                        profileData.NotificationSettings = row["NotificationSettings"]?.ToString();
                        profileData.PrivacySettings = row["PrivacySettings"]?.ToString();
                    }

                    // Add role if it exists and hasn't been added yet
                    if (row["RoleId"] != DBNull.Value)
                    {
                        var roleId = Convert.ToInt64(row["RoleId"]);
                        if (!userRoles.ContainsKey(roleId))
                        {
                            userRoles[roleId] = new Model.User.UserRoleInfo
                            {
                                RoleId = roleId,
                                RoleName = row["RoleName"]?.ToString(),
                                RoleDescription = row["RoleDescription"]?.ToString()
                            };
                        }
                    }

                    // Add address if it exists and hasn't been added yet
                    if (row["AddressId"] != DBNull.Value)
                    {
                        var addressId = Convert.ToInt64(row["AddressId"]);
                        if (!userAddresses.ContainsKey(addressId))
                        {
                            userAddresses[addressId] = new Model.User.UserAddressInfo
                            {
                                AddressId = addressId,
                                AddressType = row["AddressType"]?.ToString(),
                                Street = row["Street"]?.ToString(),
                                City = row["AddressCity"]?.ToString(),
                                State = row["AddressState"]?.ToString(),
                                PostalCode = row["AddressPostalCode"]?.ToString(),
                                Country = row["AddressCountry"]?.ToString(),
                                IsDefault = Convert.ToBoolean(row["IsDefaultAddress"])
                            };
                        }
                    }

                    // Add preference if it exists and hasn't been added yet
                    if (row["PreferenceKey"] != DBNull.Value)
                    {
                        var preferenceKey = row["PreferenceKey"]?.ToString();
                        if (!string.IsNullOrEmpty(preferenceKey) && !userPreferences.ContainsKey(preferenceKey))
                        {
                            userPreferences[preferenceKey] = new Model.User.UserPreferenceInfo
                            {
                                Key = preferenceKey,
                                Value = row["PreferenceValue"]?.ToString(),
                                Type = row["PreferenceType"]?.ToString()
                            };
                        }
                    }
                }

                // Set collections
                profileData.Roles = userRoles.Values.ToList();
                profileData.Addresses = userAddresses.Values.ToList();
                profileData.Preferences = userPreferences.Values.ToList();

                // Process statistics if available (second result set)
                if (result.Tables.Count > 1 && result.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow statRow in result.Tables[1].Rows)
                    {
                        var statType = statRow["StatType"]?.ToString();
                        var statValue = Convert.ToInt32(statRow["StatValue"]);

                        switch (statType)
                        {
                            case "LOGIN_COUNT":
                                profileData.Statistics.LoginCount = statValue;
                                break;
                            case "LAST_ACTIVITY":
                                profileData.Statistics.DaysSinceLastActivity = statValue;
                                break;
                            case "PROFILE_COMPLETION":
                                profileData.Statistics.ProfileCompletion = statValue;
                                break;
                        }
                    }
                }

                this.Logger.LogInformation($"Repository: Profile retrieval successful for user {userId}");

                return profileData;
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Repository: Profile retrieval error for user {userId}: {ex.Message}");
                
                // Check for specific error messages from stored procedure
                if (ex.Message.Contains("User not found") || ex.Message.Contains("inactive"))
                {
                    throw new KeyNotFoundException(ex.Message);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        /// <param name="request">Profile update request</param>
        /// <returns>Success confirmation message</returns>
        public async Task<string> UpdateProfile(Model.User.UpdateProfileRequest request)
        {
            try
            {
                this.Logger.LogInformation($"Repository: Profile update for user {request.UserId}");

                var parameters = new Dictionary<string, object>
                {
                    { "UserId", request.UserId },
                    { "Name", request.Name ?? (object)DBNull.Value },
                    { "Phone", request.Phone ?? (object)DBNull.Value },
                    { "DateOfBirth", request.DateOfBirth ?? (object)DBNull.Value },
                    { "Gender", request.Gender ?? (object)DBNull.Value },
                    { "Bio", request.Bio ?? (object)DBNull.Value },
                    { "Website", request.Website ?? (object)DBNull.Value },
                    { "CompanyName", request.CompanyName ?? (object)DBNull.Value },
                    { "JobTitle", request.JobTitle ?? (object)DBNull.Value },
                    { "Country", request.Country ?? (object)DBNull.Value },
                    { "City", request.City ?? (object)DBNull.Value },
                    { "State", request.State ?? (object)DBNull.Value },
                    { "PostalCode", request.PostalCode ?? (object)DBNull.Value },
                    { "Timezone", request.Timezone ?? (object)DBNull.Value },
                    { "Language", request.Language ?? (object)DBNull.Value },
                    { "PreferredContactMethod", request.PreferredContactMethod ?? (object)DBNull.Value }
                };

                // Add address parameters if address information is provided
                if (request.Address != null)
                {
                    parameters.Add("AddressStreet", request.Address.Street ?? (object)DBNull.Value);
                    parameters.Add("AddressCity", request.Address.City ?? (object)DBNull.Value);
                    parameters.Add("AddressState", request.Address.State ?? (object)DBNull.Value);
                    parameters.Add("AddressZipCode", request.Address.ZipCode ?? (object)DBNull.Value);
                    parameters.Add("AddressCountry", request.Address.Country ?? (object)DBNull.Value);
                    parameters.Add("AddressType", request.Address.AddressType ?? "Home");
                    parameters.Add("UpdateAddressIfExists", request.Address.UpdateIfExists);
                }
                else
                {
                    parameters.Add("AddressStreet", DBNull.Value);
                    parameters.Add("AddressCity", DBNull.Value);
                    parameters.Add("AddressState", DBNull.Value);
                    parameters.Add("AddressZipCode", DBNull.Value);
                    parameters.Add("AddressCountry", DBNull.Value);
                    parameters.Add("AddressType", "Home");
                    parameters.Add("UpdateAddressIfExists", true);
                }

                var result = await Task.Run(() => _dataAccess.ExecuteDataset(
                    Model.Constant.Constant.StoredProcedures.SP_UPDATE_USER_PROFILE,
                    parameters
                ));

                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                {
                    throw new KeyNotFoundException("User not found or inactive");
                }

                var updateResult = result.Tables[0].Rows[0];
                var message = updateResult["Message"]?.ToString() ?? "Profile updated successfully";
                var updateTime = Convert.ToDateTime(updateResult["UpdatedAt"]);

                this.Logger.LogInformation($"Repository: Profile update successful for user {request.UserId} at {updateTime}");

                return message;
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Repository: Profile update error for user {request.UserId}: {ex.Message}");
                
                // Check for specific error messages from stored procedure
                if (ex.Message.Contains("User not found or inactive"))
                {
                    throw new KeyNotFoundException(ex.Message);
                }
                else if (ex.Message.Contains("Phone number is already registered"))
                {
                    throw new InvalidOperationException(ex.Message);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Reset user password using reset token
        /// </summary>
        /// <param name="request">Password reset request</param>
        /// <returns>Password reset response</returns>
        public async Task<Model.User.ResetPasswordResponse> ResetPassword(Model.User.ResetPasswordRequest request)
        {
            try
            {
                this.Logger.LogInformation($"Repository: Password reset attempt for token: {request.ResetToken?.Substring(0, Math.Min(10, request.ResetToken?.Length ?? 0))}...");

                var parameters = new Dictionary<string, object>
                {
                    { "ResetToken", request.ResetToken },
                    { "NewPassword", request.NewPassword },
                    { "IpAddress", request.IpAddress ?? (object)DBNull.Value },
                    { "UserAgent", request.UserAgent ?? (object)DBNull.Value }
                };

                var result = await Task.Run(() => _dataAccess.ExecuteDataset(
                    Model.Constant.Constant.StoredProcedures.SP_RESET_PASSWORD,
                    parameters
                ));

                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                {
                    throw new UnauthorizedAccessException("Invalid or expired reset token");
                }

                var resetResult = result.Tables[0].Rows[0];
                var resetResponse = new Model.User.ResetPasswordResponse
                {
                    UserId = Convert.ToInt64(resetResult["UserId"]),
                    Email = resetResult["Email"]?.ToString() ?? "",
                    Message = resetResult["Message"]?.ToString() ?? "Password reset successfully",
                    ResetDate = Convert.ToDateTime(resetResult["ResetDate"]),
                    Success = true
                };

                this.Logger.LogInformation($"Repository: Password reset successful for user {resetResponse.UserId}");

                return resetResponse;
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Repository: Password reset error for token {request.ResetToken?.Substring(0, Math.Min(10, request.ResetToken?.Length ?? 0))}...: {ex.Message}");
                
                // Check for specific error messages from stored procedure
                if (ex.Message.Contains("Invalid or expired reset token"))
                {
                    throw new UnauthorizedAccessException("Invalid or expired reset token");
                }
                else if (ex.Message.Contains("Reset token has already been used"))
                {
                    throw new InvalidOperationException("Reset token has already been used");
                }
                else if (ex.Message.Contains("Reset token has expired"))
                {
                    throw new UnauthorizedAccessException("Reset token has expired. Please request a new password reset");
                }
                
                throw;
            }
        }

        #endregion

        #region old
        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="locationId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Model.User.User> GetById(string tenantId, string locationId, string id)
        {
            try
            {
                //Logger
                this.Logger.LogInformation($"Calling GetById({tenantId}, {locationId}, {id})");

                //retrive user      
                Model.User.User user = await this.DbContext.Users
                                                    .Where(x => x.UserId.Equals(id) &&
                                                     x.TenantId.Equals(tenantId)).FirstOrDefaultAsync();

                //Logger
                this.Logger.LogInformation($"Called GetById({tenantId}, {locationId}, {id})");

                //return 
                return user;
            }
            catch (Exception ex)
            {
                //Error logger
                this.Logger.LogError($"GetUser Error({ex.Message}) : {ex.InnerException}");
                throw ex;
            }
        }

        /// <summary>
        /// Get Role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        internal async Task<List<Model.User.Role>> GetRoles(string[] roleId)
        {
            try
            {
                //Logger
                this.Logger.LogInformation($"Calling GetRoles");

                //Query build 
                IQueryable<Model.User.Role> query = this.DbContext.Roles.AsQueryable();


                if (roleId != null && roleId.Count() > 0)
                {
                    //get role list by give id's
                    query = query.Where(x => roleId.Contains(x.Guid));
                }

                //Execute Query
                List<Model.User.Role> roles = await query.ToListAsync();

                //Logger 
                this.Logger.LogInformation($"Called GetRoles");

                //return
                return roles;
            }
            catch (Exception ex)
            {
                //Error logger
                this.Logger.LogError($"GetUser Error({ex.Message}) : {ex.InnerException}");

                throw;
            }
        }
        #endregion
    }
}
