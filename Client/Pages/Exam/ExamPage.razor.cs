using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.SignalR.Client;
using SmartProctor.Client.Components;
using SmartProctor.Client.Utils;
using SmartProctor.Client.Interops;
using SmartProctor.Shared.Responses;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamPage
    {
        [Parameter]
        public string ExamId { get; set; }

        private int _examId;

        private int _currentQuestionNum = 1;

        private ExamDetailsResponseModel _examDetails;
        private WebRTCClientTaker _webRtcClient;
        private IList<string> _proctors;
        
        private HubConnection _hubConnection;

        private bool _localDesktopVideoLoaded = false;
        private bool _localCameraVideoLoaded = false;

        private TestPrepareModal _testPrepareModal;
        private bool _inPrepare = true;

        protected override async Task OnInitializedAsync()
        {
            _examId = Int32.Parse(ExamId);
            if (await Attempt())
            {
                await GetExamDetails();
                await GetProctors();
                await SetupSignalRClient();
                SetupWebRTCClient();
                StateHasChanged();
            }
        }

        private async Task<bool> Attempt()
        {
            var (result, banReason) = await ExamServices.Attempt(_examId);

            if (result == ErrorCodes.NotLoggedIn)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "You must login first",
                });
                NavManager.NavigateTo("/User/Login");
                return false;
            }
            else if (result != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Enter test failed",
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

        private async Task GetProctors()
        {
            var (ret, proctors) = await ExamServices.GetProctors(_examId);
            if (ret == ErrorCodes.Success)
            {
                _proctors = proctors;
            }
        }

        private void SetupWebRTCClient()
        {
            _webRtcClient = new WebRTCClientTaker(JsRuntime, _proctors.ToArray());
            
            _webRtcClient.OnProctorSdp += (_, e) =>
            {
                _hubConnection.SendAsync("DesktopOffer", e.Item1, e.Item2);
            };

            _webRtcClient.OnProctorIceCandidate += (_, e) =>
            {
                _hubConnection.SendAsync("SendDesktopIceCandidate", e.Item1, e.Item2);
            };

            _webRtcClient.OnCameraSdp += (_, sdp) =>
            {
                _hubConnection.SendAsync("CameraAnswerFromTaker", sdp);
            };

            _webRtcClient.OnCameraIceCandidate += (_, candidate) =>
            {
                _hubConnection.SendAsync("CameraIceCandidateToTaker", candidate);
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

            _hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedDesktopAnswer",
                async (proctor, sdp) =>
                {
                    await _webRtcClient.ReceivedProctorAnswerSDP(proctor, sdp);
                });

            _hubConnection.On<string, RTCIceCandidate>("ReceivedDesktopIceCandidate",
                async (proctor, candidate) =>
                {
                    await _webRtcClient.ReceivedProctorIceCandidate(proctor, candidate);
                });
            _hubConnection.On<RTCIceCandidate>("CameraIceCandidateToTaker",
                async candidate =>
                {
                    await _webRtcClient.ReceivedCameraIceCandidate(candidate);
                });
            _hubConnection.On<RTCSessionDescriptionInit>("CameraOfferToTaker",
                async sdp =>
                {
                    await _webRtcClient.ReceivedCameraOfferSDP(sdp);
                });
            _hubConnection.On<string>("ProctorConnected",
                async proctor =>
                {
                    await _webRtcClient.ReconnectToProctor(proctor);
                });

            await _hubConnection.StartAsync();
        }

        private async Task OnDesktopVideoVisibleChange(bool visible)
        {
            if (visible && !_localDesktopVideoLoaded)
            {
                await _webRtcClient.SetDesktopVideoElement("local-desktop");
                _localDesktopVideoLoaded = true;
            }
        }

        private async Task OnCameraVideoVisibleChange(bool visible)
        {
            if (visible && !_localCameraVideoLoaded)
            {
                await _webRtcClient.SetCameraVideoElement("local-camera");
                _localCameraVideoLoaded = true;
            }
        }

        private async Task OnShareScreen()
        {
            var streamId = await _webRtcClient.ObtainDesktopStream();
            await _webRtcClient.SetDesktopVideoElement("desktop-video-dialog");
            if (_testPrepareModal.ShareScreenComplete(streamId))
            {
                await _webRtcClient.StartStreamingDesktop();
            }
        }

        private void OnPrepareFinish()
        {
            _inPrepare = false;
        }

        private async Task OnNextQuestion()
        {
            await ToQuestion(++_currentQuestionNum);
        }

        private async Task OnPreviousQuestion()
        {
            await ToQuestion(--_currentQuestionNum);
        }

        private async Task ToQuestion(int index)
        {
            var (res, question) = await ExamServices.GetQuestion(_examId, index);
        }

        private void OnFinish()
        {
            Modal.Warning(new ConfirmOptions()
            {
                Title = "Time's up",
            });
        }
    }
}