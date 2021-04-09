using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using SmartProctor.Client.Interops;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Responses;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class Proctor
    {
        private const int NUM_COLS = 4;
        
        private RenderFragment _logo = _ =>
        {
            // No logo, empty
        };
        
        [Inject]
        public IExamServices ExamServices { get; set; }
        
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Parameter]
        public string ExamId { get; set; }

        private int _examId;
        
        private ExamDetailsResponseModel _examDetails;
        private WebRTCClientProctor _webRtcClient;
        
        private HubConnection _hubConnection;

        private string _enlargedTestTakerName;
        private bool _enlarged;
        private bool _enlargedDesktop;
        private bool _enlargeModalLoaded;

        private string[] _testTakers = new string[0];
        private ExamTakerVideoCard[] _examTakerVideoCards = new ExamTakerVideoCard[0];
        
        protected override async Task OnInitializedAsync()
        {
            _examId = int.Parse(ExamId);
            if (await Attempt())
            {
                await GetExamDetails();
                Console.WriteLine("GetExamDetails()");
                await GetExamTakers();
                Console.WriteLine("GetExamTakers()");
                await SetupSignalRClient();
                Console.WriteLine("SetupSignalRClient()");
                SetupWebRTC();
                Console.WriteLine("SetupWebRTC()");
                await _hubConnection.SendAsync("ProctorJoin", ExamId);
                StateHasChanged();
                Console.WriteLine("_hubConnection.SendAsync(\"ProctorJoin\", ExamId);");
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
                _examTakerVideoCards = new ExamTakerVideoCard[takers.Length];
                _testTakers = takers;
                Console.WriteLine("_testTakers.Length = " + _testTakers.Length);
            }
        }

        private void SetupWebRTC()
        {
            _webRtcClient = new WebRTCClientProctor(JsRuntime, _testTakers.ToArray());
            
            _webRtcClient.OnCameraSdp += (_, e) =>
                _hubConnection.SendAsync("CameraAnswerFromProctor", e.Item1, e.Item2);
            
            _webRtcClient.OnDesktopSdp += (_, e) =>
                _hubConnection.SendAsync("DesktopAnswer", e.Item1, e.Item2);
            
            _webRtcClient.OnDesktopIceCandidate += (_, e) =>
                _hubConnection.SendAsync("SendDesktopIceCandidate", e.Item1, e.Item2);
            
            _webRtcClient.OnCameraIceCandidate += (_, e) =>
                _hubConnection.SendAsync("CameraIceCandidateFromProctor", e.Item1, e.Item2);

            _webRtcClient.OnCameraMuted += (_, e) =>
                getExamTakerVideoCard(e).CameraLoading = true;

            _webRtcClient.OnCameraUnmuted += (_, e) =>
                getExamTakerVideoCard(e).CameraLoading = false;

            _webRtcClient.OnDesktopMuted += (_, e) =>
                getExamTakerVideoCard(e).DesktopLoading = true;

            _webRtcClient.OnDesktopUnmuted += (_, e) =>
                getExamTakerVideoCard(e).DesktopLoading = false;
        }
        
        private async Task SetupSignalRClient()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/hub"))
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage",
                (testTaker, message) =>
                {
                    // TODO: Process and display message
                });

            _hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedDesktopOffer",
                async (taker, sdp) =>
                    await _webRtcClient.OnReceivedDesktopSdp(taker, sdp));

            _hubConnection.On<string, RTCIceCandidate>("ReceivedDesktopIceCandidate",
                async (taker, candidate) =>
                    await _webRtcClient.OnReceivedDesktopIceCandidate(taker, candidate));
            
            _hubConnection.On<string, RTCSessionDescriptionInit>("CameraOfferToProctor",
                async (taker, sdp) =>
                    await _webRtcClient.OnReceivedCameraSdp(taker, sdp));
            
            _hubConnection.On<string, RTCIceCandidate>("CameraIceCandidateToProctor",
                async (taker, candidate) =>
                    await _webRtcClient.OnReceivedCameraIceCandidate(taker, candidate));
            
            await _hubConnection.StartAsync();
        }

        private async Task OnToggleDesktop(string testTaker)
        {
            Console.WriteLine($"OnToggleDesktop({testTaker})");
            await _webRtcClient.SetDesktopVideoElem(testTaker, testTaker + "-video");
        }
        
        private async Task OnToggleCamera(string testTaker)
        {
            Console.WriteLine($"OnToggleCamera({testTaker})");
            await _webRtcClient.SetCameraVideoElem(testTaker, testTaker + "-video");
        }
        private async Task BanTestTaker(string testTaker)
        {
            // TODO
        }

        private ExamTakerVideoCard getExamTakerVideoCard(string testTaker)
        {
            Console.WriteLine("getExamTakerVideoCard(" + testTaker + ")");
            foreach (var i in _examTakerVideoCards)
            {
                if (i.ExamTakerName == testTaker)
                {
                    return i;
                }
            }

            return null;
        }
    }
}