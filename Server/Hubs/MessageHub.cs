using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SmartProctor.Server.Services;

namespace SmartProctor.Server.Hubs
{
    public class MessageHub : Hub
    {
        private IUserServices _userServices;
        private IExamServices _examServices;

        private IDictionary<string, string> _userGroupDict = new Dictionary<string, string>();

        public MessageHub(IExamServices examServices, IUserServices userServices)
        {
            _examServices = examServices;
            _userServices = userServices;
        }

        public async Task ExamTakerJoin(string uid, int examId)
        {
            if (_examServices.Attempt(examId, uid) != 0)
            {
                await Clients.Caller.SendAsync("ExamRejected");
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, examId + "-takers");
                await Clients.Caller.SendAsync("ExamAccepted");
                await Clients.Group(examId + "-proctors").SendAsync("ExamTakerJoined", uid);
                
                _userGroupDict.Add(Context.ConnectionId, examId.ToString());
                Context.Items["uid"] = uid;
            }
        }

        public async Task ProctorJoin(string uid, int examId)
        {
            if (_examServices.EnterProctor(examId, uid) != 0)
            {
                await Clients.Caller.SendAsync("ProctorRejected");
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, examId + "-proctors");
                await Clients.Caller.SendAsync("ProctorAccepted");
                
                _userGroupDict.Add(Context.ConnectionId, examId.ToString());
                Context.Items["uid"] = uid;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.Group(_userGroupDict[Context.ConnectionId] + "-proctors")
                .SendAsync("ExamTakerLeave", Context.Items["uid"]);
            _userGroupDict.Remove(Context.ConnectionId);
        }

        public async Task ExamTakerMessage(string message)
        {
            await Clients.Group(_userGroupDict[Context.ConnectionId] + "-proctors")
                .SendAsync("ReceiveMessage", Context.Items["uid"], message);
        }

        public async Task ProctorMessage(string message, string uid)
        {
            await Clients.User(uid).SendAsync("ReceiveMessage", "proctor", message);
        }
    }
}