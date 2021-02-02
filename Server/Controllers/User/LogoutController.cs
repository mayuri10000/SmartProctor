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
        public BaseResponseModel Get()
        {
            if (HttpContext.Session.IsAvailable && HttpContext.Session.GetString("UID") == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            HttpContext.Session.Remove("UID");
            return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success);
        }
    }

}