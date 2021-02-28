using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.SignalR.Client;
using SmartProctor.Client.WebRTCInterop;
using SmartProctor.Shared.Responses;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamPage
    {
        [Parameter]
        public string ExamId { get; set; }

        private int currentQuestionNum = 1;

        private ExamDetailsResponseModel examDetails;
        private WebRTCClientTaker _webRtcClient;
        private IList<string> _proctors;
        
        private HubConnection hubConnection;

        private bool localDesktopVideoLoaded = false;
        private bool localCameraVideoLoaded = false;

        protected override async Task OnInitializedAsync()
        {
            if (await Attempt())
            {
                await GetExamDetails();
                await GetProctors();
                await SetupSignalRClient();
                await SetupWebRTCClient();
                StateHasChanged();
            }
        }

        private async Task<bool> Attempt()
        {
            var result = await Http.GetFromJsonAsync<BaseResponseModel>("api/exam/Attempt/" + ExamId);

            if (result.Code == 1000)
            {
                Modal.Error(new ConfirmOptions()
                {
                    Title = "You must login first",
                });
                NavManager.NavigateTo("/User/Login");
                return false;
            }
            else if (result.Code != 0)
            {
                Modal.Error(new ConfirmOptions()
                {
                    Title = "Enter test failed",
                    Content = result.Message
                });
                NavManager.NavigateTo("/");
                return false;
            }

            return true;
        }

        private async Task GetExamDetails()
        {
            var details = await Http.GetFromJsonAsync<ExamDetailsResponseModel>("api/exam/ExamDetails/" + ExamId);

            if (details.Code == 0)
            {
                examDetails = details;
            }
        }

        private async Task GetProctors()
        {
            var proctors = await Http.GetFromJsonAsync<GetProctorsResponseModel>("api/exam/GetProctors/" + ExamId);
            if (proctors.Code == 0)
            {
                _proctors = proctors.Proctors;
            }
        }

        private async Task SetupWebRTCClient()
        {
            _webRtcClient = new WebRTCClientTaker(JsRuntime, _proctors.ToArray());
            
            _webRtcClient.OnProctorSdp += (_, e) =>
            {
                hubConnection.SendAsync("DesktopOffer", e.Item1, e.Item2);
            };

            _webRtcClient.OnProctorIceCandidate += (_, e) =>
            {
                hubConnection.SendAsync("SendDesktopIceCandidate", e.Item1, e.Item2);
            };

            _webRtcClient.OnCameraSdp += (_, sdp) =>
            {
                hubConnection.SendAsync("CameraAnswerFromTaker", sdp);
            };

            _webRtcClient.OnCameraIceCandidate += (_, candidate) =>
            {
                hubConnection.SendAsync("CameraIceCandidateToTaker", candidate);
            };
                
            await _webRtcClient.StartStreamingDesktop();
        }
        
        private async Task SetupSignalRClient()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/hub"))
                .Build();

            hubConnection.On<string>("ReceiveMessage",
                (message) =>
                {
                    // TODO: Process and display message
                });

            hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedDesktopAnswer",
                async (proctor, sdp) =>
                {
                    await _webRtcClient.ReceivedProctorAnswerSDP(proctor, sdp);
                });

            hubConnection.On<string, RTCIceCandidate>("ReceivedDesktopIceCandidate",
                async (proctor, candidate) =>
                {
                    await _webRtcClient.ReceivedProctorIceCandidate(proctor, candidate);
                });
            hubConnection.On<RTCIceCandidate>("CameraIceCandidateToTaker",
                async candidate =>
                {
                    await _webRtcClient.ReceivedCameraIceCandidate(candidate);
                });
            hubConnection.On<RTCSessionDescriptionInit>("CameraOfferToTaker",
                async sdp =>
                {
                    await _webRtcClient.receivedCameraOfferSDP(sdp);
                });

            await hubConnection.StartAsync();
        }

        private async Task OnDesktopVideoVisibleChange(bool visible)
        {
            if (visible && !localDesktopVideoLoaded)
            {
                await _webRtcClient.SetDesktopVideoElement("local-desktop");
                localDesktopVideoLoaded = true;
            }
        }

        private async Task OnCameraVideoVisibleChange(bool visible)
        {
            if (visible && !localCameraVideoLoaded)
            {
                await _webRtcClient.SetCameraVideoElement("local-camera");
                localCameraVideoLoaded = true;
            }
        }

        private void OnNextQuestion()
        {
            ToQuestion(++currentQuestionNum);
        }

        private void OnPreviousQuestion()
        {
            ToQuestion(--currentQuestionNum);
        }

        private void ToQuestion(int index)
        {
            // TODO: Display questions
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