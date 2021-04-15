using System;
using System.Collections.Generic;
using System.Linq;
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
        private IExamServices _examServices;

        public MessageHub(IExamServices examServices, IUserServices userServices)
        {
            _examServices = examServices;
        }

        

        public async Task ProctorJoin(string examId)
        {
            var examTakers = _examServices.GetExamTakers(int.Parse(examId)).Select(x => x.Item1);
            if (examTakers != null)
            {
                foreach (var taker in examTakers)
                {
                    await Clients.User(taker).SendAsync("ProctorConnected", Context.UserIdentifier);
                }
            }
        }
        

        public async Task ProctorMessageIndividual(string user, string message)
        {
            await Clients.User(user).SendAsync("ReceiveMessage", Context.UserIdentifier, message);
        }

        public async Task TestTakerMessage(string examId, string messageType, string message)
        {
            var uid = Context.UserIdentifier;
            if (uid.EndsWith("_cam"))
            {
                uid = uid.Substring(0, Context.UserIdentifier.Length - 4);
            }
            
            var proctors = _examServices.GetProctors(int.Parse(examId));
            if (proctors != null)
            {
                foreach (var proctor in proctors)
                {
                    await Clients.User(proctor)
                        .SendAsync("ReceiveMessage", uid, messageType, message);
                }
            }
        }
        
        public async Task ProctorMessageGroup(string examId, string messageType, string message)
        {
            var examTakers = _examServices.GetExamTakers(int.Parse(examId)).Select(x => x.Item1);
            if (examTakers != null)
            {
                foreach (var taker in examTakers)
                {
                    await Clients.User(taker).SendAsync("ReceiveMessage", Context.UserIdentifier, message);
                }
            }
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
        
        public async Task ExamEnded()
        {
            var user = Context.User.Identity.Name + "_cam";
            await Clients.User(user).SendAsync("ExamEnded");
        }
    }
}