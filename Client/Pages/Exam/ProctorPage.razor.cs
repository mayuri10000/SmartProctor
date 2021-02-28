using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using SmartProctor.Client.WebRTCInterop;
using SmartProctor.Shared.Responses;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ProctorPage
    {
        [Parameter]
        public string ExamId { get; set; }
        
        private ExamDetailsResponseModel examDetails;
        private WebRTCClientProctor _webRtcClient;
        private IList<string> _testTakers;
        
        private HubConnection hubConnection;
        
        protected override async Task OnInitializedAsync()
        {
            if (await Attempt())
            {
                await GetExamDetails();
                await GetExamTakers();
                await SetupWebRTC();
                await SetupSignalRClient();
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
                    Title = "Enter proctor failed",
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

        private async Task GetExamTakers()
        {
            var takers = await Http.GetFromJsonAsync<GetExamTakersResponseModel>("api/exam/GetExamTakers/" + ExamId);
            if (takers.Code == 0)
            {
                _testTakers = takers.ExamTakers;
            }
        }

        private async Task SetupWebRTC()
        {
            _webRtcClient = new WebRTCClientProctor(JsRuntime, _testTakers.ToArray());
            _webRtcClient.OnCameraSdp += (_, e) =>
            {
                hubConnection.SendAsync("CameraAnswerFromProctor", e.Item1, e.Item2);
            };
            _webRtcClient.OnDesktopSdp += (_, e) =>
            {
                hubConnection.SendAsync("ReceivedDesktopAnswer", e.Item1, e.Item2);
            };
            _webRtcClient.OnDesktopIceCandidate += (_, e) =>
            {
                hubConnection.SendAsync("ReceivedDesktopIceCandidate", e.Item1, e.Item2);
            };
            _webRtcClient.OnCameraIceCandidate += (_, e) =>
            {
                hubConnection.SendAsync("CameraIceCandidateFromProctor", e.Item1, e.Item2);
            };

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

            hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedDesktopOffer",
                async (taker, sdp) =>
                {
                    await _webRtcClient.OnReceivedDesktopSdp(taker, sdp);
                });

            hubConnection.On<string, RTCIceCandidate>("ReceivedDesktopIceCandidate",
                async (taker, candidate) =>
                {
                    await _webRtcClient.OnReceivedDesktopIceCandidate(taker, candidate);
                });
            hubConnection.On<string, RTCSessionDescriptionInit>("CameraOfferToProctor",
                async (taker, sdp) =>
                {
                    await _webRtcClient.OnReceivedCameraSdp(taker, sdp);
                });
            hubConnection.On<string, RTCIceCandidate>("CameraIceCandidateToProctor",
                async (taker, candidate) =>
                {
                    await _webRtcClient.OnReceivedCameraIceCandidate(taker, candidate);
                });
            await hubConnection.StartAsync();
        }

    }
}