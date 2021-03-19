using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class UserDetailsController : ControllerBase
    {
        private IUserServices _services;

        public UserDetailsController(IUserServices services)
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
            
            var u = _services.GetObject(uid);
            return new UserDetailsResponseModel()
            {
                Code = 0,
                Id = u.Id,
                NickName = u.NickName,
                Email = u.Email,
                Phone = u.Phone
            };
        }
    }
}