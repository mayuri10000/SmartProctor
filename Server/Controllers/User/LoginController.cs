using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class LoginController : ControllerBase
    {
        private IUserServices _services;

        public LoginController(IUserServices services)
        {
            _services = services;
        }

        [HttpPost]
        public async Task<BaseResponseModel> Post(LoginRequestModel model)
        {
            var uid = _services.Login(model.UserName, model.Password);
            if (uid == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.UserNameOrPasswordWrong);
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, uid),
                new Claim(ClaimTypes.NameIdentifier, uid),
                new Claim(ClaimTypes.Role, "User"),
            };
            
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = model.Remember
                });
            
            return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success);
        }
    }
}