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
