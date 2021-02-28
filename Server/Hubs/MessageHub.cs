using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SmartProctor.Server.Data.Entities;
using SmartProctor.Server.Services;
using SmartProctor.Server.Utils;
using SmartProctor.Shared.WebRTC;

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

        public async Task Test()
        {
            await Clients.Caller.SendAsync("TestBack",
                Context.User?.Identity == null ? "null" : Context.User.Identity.Name);
        }

        public async Task ExamTakerJoin(int examId)
        {
            if (Context.User?.Identity?.Name == null)
            {
                await Clients.Caller.SendAsync("Error", ErrorCodes.NotLoggedIn);
                return;
            }

            var ret = _examServices.Attempt(examId, Context.User.Identity.Name);
            if (ret != 0)
            {
                await Clients.Caller.SendAsync("Error", ret);
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, examId + "-takers");
                await Clients.Caller.SendAsync("ExamAccepted");
                await Clients.Group(examId + "-proctors").SendAsync("ExamTakerJoined", Context.User.Identity.Name);
                
                _userGroupDict.Add(Context.ConnectionId, examId.ToString());
            }
        }

        public async Task ProctorJoin(int examId)
        {
            if (Context.User?.Identity?.Name == null)
            {
                await Clients.Caller.SendAsync("Error", ErrorCodes.NotLoggedIn);
                return;
            }

            var ret = _examServices.EnterProctor(examId, Context.User.Identity.Name);
            if (ret != 0)
            {
                await Clients.Caller.SendAsync("Error", ret);
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, examId + "-proctors");
                await Clients.Caller.SendAsync("ProctorAccepted");
                
                _userGroupDict.Add(Context.ConnectionId, examId.ToString());
            }
        }

        public async Task DeepLensJoin(int examId)
        {
            if (Context.User?.Identity?.Name == null)
            {
                await Clients.Caller.SendAsync("Error", ErrorCodes.NotLoggedIn);
                return;
            }

            var ret = _examServices.Attempt(examId, Context.User.Identity.Name);
            if (ret != 0)
            {
                await Clients.Caller.SendAsync("Error", ret);
            }
            else
            {
                var un = Context.User.Identity.Name.Substring(0, Context.User.Identity.Name.Length - 4);
                
                await Groups.AddToGroupAsync(Context.ConnectionId, examId + "-deeplens");
                await Clients.Caller.SendAsync("DeepLensJoined");
                await Clients.User(un)
                    .SendAsync("DeepLensJoined");
                await Clients.Group(examId + "-proctors").SendAsync("DeepLensJoined", un);
                
                _userGroupDict.Add(Context.ConnectionId, examId.ToString());
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_userGroupDict.ContainsKey(Context.ConnectionId))
            {
                await Clients.Group(_userGroupDict[Context.ConnectionId] + "-proctors")
                    .SendAsync("ExamTakerLeave", Context.Items["uid"]);
                _userGroupDict.Remove(Context.ConnectionId);
            }
        }

        public async Task ExamTakerMessage(string message)
        {
            if (_userGroupDict.ContainsKey(Context.ConnectionId))
                await Clients.Group(_userGroupDict[Context.ConnectionId] + "-proctors")
                    .SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }

        public async Task ProctorMessage(string message, string uid)
        {
            if (_userGroupDict.ContainsKey(Context.ConnectionId))
                await Clients.User(uid).SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }
        
        public async Task ProctorMessageGroup(string message)
        {
            if (_userGroupDict.ContainsKey(Context.ConnectionId))
                await Clients.Group(_userGroupDict[Context.ConnectionId] + "-takers")
                    .SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }

        public async Task DesktopOffer(string proctor, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(proctor).SendAsync("ReceivedDesktopOffer", Context.User.Identity.Name, sdp);
        }

        public async Task DesktopAnswer(string testTaker, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(testTaker).SendAsync("ReceivedDesktopAnswer", Context.User.Identity.Name, sdp);
        }

        public async Task SendDesktopIceCandidate(string testTaker, RTCIceCandidate candidate)
        {
            await Clients.User(testTaker).SendAsync("ReceivedDesktopIceCandidate", Context.User.Identity.Name, candidate);
        }

        public async Task CameraOfferToTaker(RTCSessionDescriptionInit sdp)
        {
            var user = Context.User.Identity.Name.Substring(0, Context.User.Identity.Name.Length - 4);
            await Clients.User(user).SendAsync("CameraOfferToTaker", sdp);
        }
        
        public async Task CameraAnswerFromTaker(RTCSessionDescriptionInit sdp)
        {
            var user = Context.User.Identity.Name + "_cam";
            await Clients.User(user).SendAsync("CameraAnswerFromTaker", sdp);
        }
        
        public async Task CameraIceCandidateToTaker(RTCIceCandidate candidate)
        {
            var user = Context.User.Identity.Name.Substring(0, Context.User.Identity.Name.Length - 4);
            await Clients.User(user).SendAsync("CameraIceCandidateToTaker", candidate);
        }
        
        public async Task CameraIceCandidateFromTaker(RTCIceCandidate candidate)
        {
            var user = Context.User.Identity.Name + "_cam";
            await Clients.User(user).SendAsync("CameraIceCandidateFromTaker", candidate);
        }
        
        public async Task CameraOfferToProctor(string proctor, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(proctor).SendAsync("CameraOfferToProctor", Context.User.Identity.Name, sdp);
        }
        
        public async Task CameraAnswerFromProctor(string testTaker, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(testTaker).SendAsync("CameraAnswerFromTaker", Context.User.Identity.Name, sdp);
        }
        
        public async Task CameraIceCandidateToProctor(string proctor, RTCIceCandidate candidate)
        {
            await Clients.User(proctor).SendAsync("CameraIceCandidateToProctor", Context.User.Identity.Name, candidate);
        }
        
        public async Task CameraIceCandidateFromProctor(string testTaker, RTCIceCandidate candidate)
        {
            await Clients.User(testTaker).SendAsync("CameraIceCandidateFromProctor", Context.User.Identity.Name, candidate);
        }
    }
}