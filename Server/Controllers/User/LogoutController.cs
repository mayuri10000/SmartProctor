using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class LogoutController : ControllerBase
    {
        [HttpGet]
        public async Task<BaseResponseModel> Get()
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success);
        }
    }

}