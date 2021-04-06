﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using SmartProctor.Client.Interops;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamDropdown
    {
        [Inject]
        public IExamServices ExamServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Parameter]
        public int ExamId { get; set; }

        private WebRTCClientTaker _webRtcClient;
        private HubConnection _hubConnection;
        private IList<string> _proctors;
        
        private bool _localDesktopVideoLoaded = false;
        private bool _localCameraVideoLoaded = false;
        private bool _cameraVideoLoading = true;
        private bool _inPrepare = true;

        private TestPrepareModal _testPrepareModal;

        protected override async Task OnInitializedAsync()
        {
            if (await Attempt())
            {
                await GetProctors();
                await SetupSignalRClient();
                SetupWebRTCClient();
                StateHasChanged();
            }
        }
        
        private async Task GetProctors()
        {
            var (ret, proctors) = await ExamServices.GetProctors(ExamId);
            if (ret == ErrorCodes.Success)
            {
                _proctors = proctors;
            }
        }
        
        private async Task<bool> Attempt()
        {
            var (result, banReason) = await ExamServices.Attempt(ExamId);

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
            
            _webRtcClient.OnCameraConnectionStateChange += (_, state) =>
            {
                _cameraVideoLoading = state != "connected";
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
    }
}