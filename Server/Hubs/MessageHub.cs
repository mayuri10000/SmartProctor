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
                Context.UserIdentifier ?? "null");
        }

        public async Task ProctorJoin(string examId)
        {
            var examTakers = _examServices.GetExamTakers(int.Parse(examId));
            if (examTakers != null)
            {
                foreach (var taker in examTakers)
                {
                    await Clients.User(taker).SendAsync("ProctorConnected", Context.UserIdentifier);
                }
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