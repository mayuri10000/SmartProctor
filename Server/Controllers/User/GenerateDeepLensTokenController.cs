using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class GenerateDeepLensTokenController : ControllerBase
    {
        private IUserServices _services;

        public GenerateDeepLensTokenController(IUserServices services)
        {
            _services = services;
        }

        [HttpGet]
        public BaseResponseModel Get()
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var uid = User.Identity.Name;
            
            var u = _services.GenerateOneTimeToken(uid);
            return new DeepLensTokenResponseModel()
            {
                Code = 0,
                Message = "Success",
                Token = u
            };
        }
    }
}