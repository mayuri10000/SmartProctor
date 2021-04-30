using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartProctor.Server.Hubs;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class BanExamTakerController : ControllerBase
    {
        private IExamServices _services;
        private IHubContext<MessageHub> _hubContext;

        public BanExamTakerController(IExamServices services, IHubContext<MessageHub> hubContext)
        {
            _services = services;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<BaseResponseModel> Post(BanExamTakerRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }
            
            var uid = User.Identity.Name;

            var res = _services.BanExamTaker(model.ExamId, uid, model.UserId, model.Reason);

            if (res == ErrorCodes.Success)
            {
                await _hubContext.Clients.User(model.UserId).SendAsync("ExamTakerBanned", model.Reason);
            }

            return ErrorCodes.CreateSimpleResponse(res);
        } 
    }
}