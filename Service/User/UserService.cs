using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenant.API.Base.Repository;
using Tenant.API.Base.Service;
using Tenant.API.Base.Model;

namespace Tenant.Query.Service.User
{
    public class UserService : TnBaseService
    {
        #region Variables

        private Repository.User.UserRepository UserRepository;

        #endregion

        public UserService(Repository.User.UserRepository userRepository, ILoggerFactory loggerFactory, TnAudit xcAudit, TnValidation xcValidation) : base(xcAudit, xcValidation)
        {
            this.UserRepository = userRepository;
            this.UserRepository.Logger = loggerFactory.CreateLogger<Repository.User.UserRepository>();
        }

        #region new
        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="tenantId">Tenant identifier.</param>
        public List<Model.User.UserDetails> GetUserWithAddress(long userId)
        {
            try
            {
                List<Model.User.UserDetails> users = new List<Model.User.UserDetails>();

                // Fetch user details
                List<Model.User.SpUserMasterList> spUserMasterList = this.UserRepository.GetUser("SP_CUSTOMER", userId);

                // Map SpUserMasterList to UserDetails and include address details
                if (spUserMasterList != null)
                {
                    users.AddRange(spUserMasterList.Select(spUser => new Model.User.UserDetails
                    {
                        UserId = spUser.UserId,
                        FirstName = spUser.FirstName,
                        LastName = spUser.LastName,
                        Email = spUser.Email,
                        Phone = spUser.Phone,
                        SystemAdmin=spUser.SystemAdmin,
                        CreatedAt = spUser.CreatedAt,
                        Addresses = new List<Model.User.Address>
                        {
                            new Model.User.Address
                            {
                                AddressType = spUser.AddressType,
                                Street = spUser.Street,
                                City = spUser.City,
                                State = spUser.State,
                                PostalCode = spUser.PostalCode,
                                Country = spUser.Country,
                                IsDefault = spUser.IsDefault
                            }
                        }
                    }));
                }

                return users;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region old



        /// <summary>
        /// Get list of roles
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        internal List<Model.User.Role> GetRoles(string[] roleId)
        {
            try
            {
                //Logger 
                this.Logger.LogInformation($"Calling GetRoles");

                //Local variable
                List<Model.User.Role> roles = new List<Model.User.Role>();

                //Get roles
                roles = this.UserRepository.GetRoles(roleId).Result;

                //Logger
                this.Logger.LogInformation($"Called GetRoles");

                //return 
                return roles;
            }
            catch (System.Exception ex)
            {
                //Error logger
                this.Logger.LogError($"GetUser Error({ex.Message}) : {ex.InnerException}");
                throw;
            }
        }
        #endregion
    }
}
