using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    /// <summary>
    /// Controller used for modify the password of the user. 
    /// </summary>
    [ApiController]
    [Route("api/user/[controller]")]
    public class ModifyPasswordController : ControllerBase
    {
        private readonly IUserServices _services;

        public ModifyPasswordController(IUserServices services)
        {
            _services = services;
        }

        [HttpPost]
        public BaseResponseModel Post(ModifyPasswordRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var uid = User.Identity.Name;
            
            return ErrorCodes.CreateSimpleResponse(_services.ChangePassword(uid, model.OldPassword, model.NewPassword));
        }
    }
}