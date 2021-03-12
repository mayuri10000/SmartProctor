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

        private bool loading = true;
        
        private string enlargedTestTakerName;
        private bool enlarged = true;
        private bool enlargedDesktop;
        
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
            if (await Attempt())
            {
                await GetExamDetails();
                await GetExamTakers();
                await SetupSignalRClient();
                await SetupWebRTC();
                await hubConnection.SendAsync("ProctorJoin", ExamId);
                StateHasChanged();
            }

            loading = false;
            enlarged = false;
        }

        private async Task<bool> Attempt()
        {
            var result = await Http.GetFromJsonAsync<BaseResponseModel>("api/exam/EnterProctor/" + ExamId);

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
                hubConnection.SendAsync("DesktopAnswer", e.Item1, e.Item2);
            };
            _webRtcClient.OnDesktopIceCandidate += (_, e) =>
            {
                hubConnection.SendAsync("SendDesktopIceCandidate", e.Item1, e.Item2);
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
            enlargedTestTakerName = testTaker;
            enlarged = true;
            StateHasChanged();
            if (enlargedDesktop)
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
            if (!enlargedDesktop)
            {
                await _webRtcClient.SetDesktopVideoElem(enlargedTestTakerName, "video-enlarged");
                enlargedDesktop = true;
            }
            else
            {
                await _webRtcClient.SetCameraVideoElem(enlargedTestTakerName, "video-enlarged");
                enlargedDesktop = false;
            }
        }

        private async Task CancelEnlarged()
        {
            if (enlargedDesktop)
            {
                await _webRtcClient.SetDesktopVideoElem(enlargedTestTakerName, enlargedTestTakerName + "-video");
            }
            else
            {
                await _webRtcClient.SetCameraVideoElem(enlargedTestTakerName, enlargedTestTakerName + "-video");
            }

            enlarged = false;
        }
    }
}