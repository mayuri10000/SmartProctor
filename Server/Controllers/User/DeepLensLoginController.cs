using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    /// <summary>
    /// Controller used by the AWS DeepLens edge computing client to log in to the server, should use a
    /// token generated on the server.
    /// </summary>
    [ApiController]
    [Route("api/user/[controller]")]
    public class DeepLensLoginController : ControllerBase
    {
        private IUserServices _services;

        public DeepLensLoginController(IUserServices services)
        {
            _services = services;
        }

        [HttpGet("{token}")]
        public async Task<BaseResponseModel> Get(string token)
        {
            var uid = _services.ValidateOneTimeToken(token);
            if (uid == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.UserNameOrPasswordWrong);
            }
            
            var claims = new List<Claim>
            {
                // The user name should end with {user name}_cam, 
                new Claim(ClaimTypes.Name, uid + "_cam"),
                new Claim(ClaimTypes.NameIdentifier, uid + "_cam"),
                new Claim(ClaimTypes.Role, "DeepLens"),
            };
            
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Adds the identity information to the Cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
            
            return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success);
        }
    }
}