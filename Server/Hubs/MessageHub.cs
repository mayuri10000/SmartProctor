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
    /// <summary>
    /// SignalR hub used for messaging and WebRTC signaling.
    /// Note that this hub is also used in <see cref="SmartProctor.Server.Controllers.Exam.BanExamTakerController"/>
    /// and <see cref="SmartProctor.Server.Controllers.Exam.SendEventController"/>
    /// </summary>
    public class MessageHub : Hub
    {
        private IExamServices _examServices;

        public MessageHub(IExamServices examServices)
        {
            _examServices = examServices;
        }
        
        /// <summary>
        /// (Used by proctors) 
        /// Called when a proctor joins an exam, send message to exam takers
        /// </summary>
        /// <param name="examId"></param>
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
        
        /// <summary>
        /// (Used by exam takers) 
        /// Send the WebRTC SDP offer of the desktop stream to the proctor
        /// </summary>
        /// <param name="proctor">The user ID of the proctor to send the SDP</param>
        /// <param name="sdp">The SDP offer</param>
        public async Task DesktopOffer(string proctor, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(proctor).SendAsync("ReceivedDesktopOffer", Context.User.Identity.Name, sdp);
        }

        /// <summary>
        /// (Used by proctors) Send the WebRTC SDP answer of the desktop stream back to the exam takers
        /// </summary>
        /// <param name="testTaker">The user ID of the exam taker to send the SDP</param>
        /// <param name="sdp">The SDP answer</param>
        public async Task DesktopAnswer(string testTaker, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(testTaker).SendAsync("ReceivedDesktopAnswer", Context.User.Identity.Name, sdp);
        }

        /// <summary>
        /// (Used by both takers and proctors) Send the WebRTC ICE candidate information of the desktop stream
        /// </summary>
        /// <param name="user">The user ID of the user to send the ICE candidate</param>
        /// <param name="candidate">The ICE candidate to be sent</param>
        public async Task DesktopIceCandidate(string user, RTCIceCandidate candidate)
        {
            await Clients.User(user).SendAsync("ReceivedDesktopIceCandidate", Context.User.Identity.Name, candidate);
        }

        /// <summary>
        /// (Used by exam takers) 
        /// Send the WebRTC SDP offer of the camera stream to the proctor
        /// </summary>
        /// <param name="proctor">The user ID of the proctor to send the SDP</param>
        /// <param name="sdp">The SDP offer</param>
        public async Task CameraOffer(string proctor, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(proctor).SendAsync("ReceivedCameraOffer", Context.User.Identity.Name, sdp);
        }
        
        /// <summary>
        /// (Used by proctors) Send the WebRTC SDP answer of the camera stream back to the exam takers
        /// </summary>
        /// <param name="testTaker">The user ID of the exam taker to send the SDP</param>
        /// <param name="sdp">The SDP answer</param>
        public async Task CameraAnswer(string testTaker, RTCSessionDescriptionInit sdp)
        {
            await Clients.User(testTaker).SendAsync("ReceivedCameraAnswer", Context.User.Identity.Name, sdp);
        }
        
        /// <summary>
        /// (Used by both takers and proctors) Send the WebRTC ICE candidate information of the camera stream
        /// </summary>
        /// <param name="user">The user ID of the user to send the ICE candidate</param>
        /// <param name="candidate">The ICE candidate to be sent</param>
        public async Task CameraIceCandidate(string user, RTCIceCandidate candidate)
        {
            await Clients.User(user).SendAsync("ReceivedCameraIceCandidate", Context.User.Identity.Name, candidate);
        }
    }
}