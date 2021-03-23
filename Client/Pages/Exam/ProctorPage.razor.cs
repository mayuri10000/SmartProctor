using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using SmartProctor.Client.Utils;
using SmartProctor.Client.Interops;
using SmartProctor.Shared.Responses;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ProctorPage
    {
        [Parameter]
        public string ExamId { get; set; }

        private int _examId;
        
        private ExamDetailsResponseModel _examDetails;
        private WebRTCClientProctor _webRtcClient;
        private IList<string> _testTakers;
        
        private HubConnection _hubConnection;

        private string _enlargedTestTakerName;
        private bool _enlarged;
        private bool _enlargedDesktop;
        private bool _enlargeModalLoaded;
        
        private ListGridType gutter = new ListGridType
        {
            Gutter = 16,
            Xs = 1,
            Sm = 2,
            Md = 4,
            Lg = 4,
            Xl = 6,
            Xxl = 3,
            Column = 3
        };
        
        protected override async Task OnInitializedAsync()
        {
            _examId = Int32.Parse(ExamId);
            if (await Attempt())
            {
                await GetExamDetails();
                await GetExamTakers();
                await SetupSignalRClient();
                SetupWebRTC();
                await _hubConnection.SendAsync("ProctorJoin", ExamId);
                StateHasChanged();
            }
        }

        private async Task<bool> Attempt()
        {
            var result = await ExamServices.AttemptProctor(_examId);

            if (result == ErrorCodes.NotLoggedIn)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "You must login first",
                });
                NavManager.NavigateTo("/User/Login");
                return false;
            }
            
            if (result != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Enter proctor failed",
                    Content = ErrorCodes.MessageMap[result]
                });
                NavManager.NavigateTo("/");
                return false;
            }

            return true;
        }

        private async Task GetExamDetails()
        {
            var (ret, details) = await ExamServices.GetExamDetails(_examId);

            if (ret == ErrorCodes.Success)
            {
                _examDetails = details;
            }
        }

        private async Task GetExamTakers()
        {
            var (err, takers) = await ExamServices.GetTestTakers(_examId);
            if (err == ErrorCodes.Success)
            {
                _testTakers = takers;
            }
        }

        private void SetupWebRTC()
        {
            _webRtcClient = new WebRTCClientProctor(JsRuntime, _testTakers.ToArray());
            _webRtcClient.OnCameraSdp += (_, e) =>
            {
                _hubConnection.SendAsync("CameraAnswerFromProctor", e.Item1, e.Item2);
            };
            _webRtcClient.OnDesktopSdp += (_, e) =>
            {
                _hubConnection.SendAsync("DesktopAnswer", e.Item1, e.Item2);
            };
            _webRtcClient.OnDesktopIceCandidate += (_, e) =>
            {
                _hubConnection.SendAsync("SendDesktopIceCandidate", e.Item1, e.Item2);
            };
            _webRtcClient.OnCameraIceCandidate += (_, e) =>
            {
                _hubConnection.SendAsync("CameraIceCandidateFromProctor", e.Item1, e.Item2);
            };

        }
        
        private async Task SetupSignalRClient()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/hub"))
                .Build();

            _hubConnection.On<string>("ReceiveMessage",
                (message) =>
                {
                    // TODO: Process and display message
                });

            _hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedDesktopOffer",
                async (taker, sdp) =>
                {
                    await _webRtcClient.OnReceivedDesktopSdp(taker, sdp);
                });

            _hubConnection.On<string, RTCIceCandidate>("ReceivedDesktopIceCandidate",
                async (taker, candidate) =>
                {
                    await _webRtcClient.OnReceivedDesktopIceCandidate(taker, candidate);
                });
            _hubConnection.On<string, RTCSessionDescriptionInit>("CameraOfferToProctor",
                async (taker, sdp) =>
                {
                    await _webRtcClient.OnReceivedCameraSdp(taker, sdp);
                });
            _hubConnection.On<string, RTCIceCandidate>("CameraIceCandidateToProctor",
                async (taker, candidate) =>
                {
                    await _webRtcClient.OnReceivedCameraIceCandidate(taker, candidate);
                });
            await _hubConnection.StartAsync();
        }

        private async Task OnToggleDesktop(string testTaker)
        {
            await _webRtcClient.SetDesktopVideoElem(testTaker, testTaker + "-video");
        }
        
        private async Task OnToggleCamera(string testTaker)
        {
            await _webRtcClient.SetCameraVideoElem(testTaker, testTaker + "-video");
        }

        private async Task OnEnlarge(string testTaker)
        {
            _enlargedTestTakerName = testTaker;
            _enlarged = true;
            
            if (!_enlargeModalLoaded)
            {
                // Modal content will not be rendered before 
                // it is made visible, referencing its content will cause error
                await Task.Delay(300);
                _enlargeModalLoaded = true;
            }
            StateHasChanged();
            if (_enlargedDesktop)
            {
                await _webRtcClient.SetDesktopVideoElem(testTaker, "video-enlarged");
            }
            else
            {
                await _webRtcClient.SetCameraVideoElem(testTaker, "video-enlarged");
            }
        }

        private async Task ToggleEnlarged()
        {
            if (!_enlargedDesktop)
            {
                await _webRtcClient.SetDesktopVideoElem(_enlargedTestTakerName, "video-enlarged");
                _enlargedDesktop = true;
            }
            else
            {
                await _webRtcClient.SetCameraVideoElem(_enlargedTestTakerName, "video-enlarged");
                _enlargedDesktop = false;
            }
        }

        private async Task CancelEnlarged()
        {
            if (_enlargedDesktop)
            {
                await _webRtcClient.SetDesktopVideoElem(_enlargedTestTakerName, _enlargedTestTakerName + "-video");
            }
            else
            {
                await _webRtcClient.SetCameraVideoElem(_enlargedTestTakerName, _enlargedTestTakerName + "-video");
            }

            _enlarged = false;
        }
    }
}