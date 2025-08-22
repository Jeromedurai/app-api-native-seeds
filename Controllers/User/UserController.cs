using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Tenant.API.Base.Attribute;
using Tenant.API.Base.Controller;
using Tenant.API.Base.Model;
using Tenant.Query.Service;

namespace Tenant.Query.Controllers
{
    [Route("api/user")]
    public class UserController : TnBaseController<Service.User.UserService>
    {
        public UserController(Service.User.UserService service, IConfiguration configuration, ILoggerFactory loggerFactory) : base(service, configuration, loggerFactory)
        {

        }

        #region Get

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <returns>The user.</returns>
        /// <param name="userId">User identifier.</param>
        /// <param name="tenantId">Tenant identifier.</param>        
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.User.User))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        [Route("tenants/{userId:long}")]
        public IActionResult GetUser([FromRoute] long userId)
        {
            try
            {
                //Getting user information 
                List<Model.User.UserDetails> users = this.Service.GetUserWithAddress(userId);

                //return 
                if (users != null)
                    return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = users });
                else
                    return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Data = $"User does not exists with id '{userId}'" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
            }
        }

        [HttpGet]
        [Route("tenants/{tenantId}/user-role")]
        [SwaggerResponse(StatusCodes.Status200OK,"", typeof(List<Model.User.Role>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError,"", typeof(ApiResult))]
        public IActionResult GetRoles([FromRoute] string tenantId, [FromQuery] string[] role)
        {
            try
            {
                //Getting roles 
                List<Model.User.Role> roles = this.Service.GetRoles(role);

                //return 
                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = roles });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
            }
        }
        #endregion
    }
}