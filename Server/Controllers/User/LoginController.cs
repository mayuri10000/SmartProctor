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
        public BaseResponseModel Post(LoginRequestModel model)
        {
            var uid = _services.Login(model.UserName, model.Password);
            if (uid == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.UserNameOrPasswordWrong);
            }

            HttpContext.Session.SetString("UID", uid);
            return ErrorCodes.CreateSimpleResponse(ErrorCodes.Success);
        }
    }
}