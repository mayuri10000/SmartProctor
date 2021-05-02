using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartProctor.Server.Hubs;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Server.Controllers.Exam
{
    [ApiController]
    [Route("api/exam/[controller]")]
    public class SendEventController : ControllerBase
    {
        private IHubContext<MessageHub> _hubContext;
        private IExamServices _examServices;

        public SendEventController(IExamServices examServices, IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
            _examServices = examServices;
        }

        [HttpPost]
        public async Task<BaseResponseModel> Post(SendEventRequestModel model)
        {
            if (User.Identity?.Name == null)
            {
                return ErrorCodes.CreateSimpleResponse(ErrorCodes.NotLoggedIn);
            }

            var uid = User.Identity.Name;

            // Removes the "_cam" prefix for the DeepLens users
            if (User.IsInRole("DeepLens"))
            {
                uid = uid[..^4];
            }

            // Only proctors can sent one-to-one message, other messages will be broadcast messages
            if (model.Type != Consts.MessageTypeProctor)
            {
                model.Receipt = null;
            }

            var res = _examServices.AddEvent(model.ExamId, uid, model.Receipt, model.Type, model.Message,
                model.Attachment);

            if (res == ErrorCodes.Success)
            {
                var takers = _examServices.GetExamTakers(model.ExamId).Select(x => x.Item1).ToList();
                var proctors = _examServices.GetProctors(model.ExamId);

                if (model.Type is Consts.MessageTypeTaker or Consts.MessageTypeWarning)
                {
                    foreach (var proctor in proctors)
                    {
                        await _hubContext.Clients.User(proctor).SendAsync("ReceivedMessage", new EventItem()
                        {
                            Type = model.Type,
                            Sender = uid,
                            Receipt = model.Receipt,
                            Message = model.Message,
                            Attachment = model.Attachment,
                            Time = DateTime.Now
                        });
                    }
                }
                else if (model.Type == Consts.MessageTypeProctor)
                {
                    if (model.Receipt != null)
                    {
                        await _hubContext.Clients.User(model.Receipt).SendAsync("ReceivedMessage", new EventItem()
                        {
                            Type = model.Type,
                            Sender = uid,
                            Receipt = model.Receipt,
                            Message = model.Message,
                            Attachment = model.Attachment,
                            Time = DateTime.Now
                        });
                    }
                    else
                    {
                        foreach (var taker in takers)
                        {
                            await _hubContext.Clients.User(taker).SendAsync("ReceivedMessage", new EventItem()
                            {
                                Type = model.Type,
                                Sender = uid,
                                Receipt = model.Receipt,
                                Message = model.Message,
                                Attachment = model.Attachment,
                                Time = DateTime.Now
                            });
                        }
                    }
                }
            }

            return ErrorCodes.CreateSimpleResponse(res);
        }
    }
}