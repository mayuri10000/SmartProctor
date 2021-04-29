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
        
        [Inject]
        public NotificationService Notification { get; set; }
        
        [Parameter]
        public string ExamId { get; set; }

        private int _examId;
        
        private ExamDetailsResponseModel _examDetails;
        private WebRTCClientProctor _webRtcClient;
        
        private HubConnection _hubConnection;

        private bool _banModalVisible = false;
        private string _banTakerName = "";
        private string _banReason;
        
        private IList<string> _banReasonOptions = new List<string>()
        {
            "Leaving the exam environment",
            "Have other people in the exam environment",
            "Using cellphones",
            "Use books in closed-book exam",
            "Leave the exam page during exam",
            "Not attempted the exam in time"
        };

        private IList<UserBasicInfo> _testTakers = new List<UserBasicInfo>();
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
                _examTakerVideoCards = new ExamTakerVideoCard[takers.Count];
                _testTakers = takers;
            }
        }

        private void SetupWebRTC()
        {
            _webRtcClient = new WebRTCClientProctor(JsRuntime, _testTakers.Select(x => x.Id).ToArray());
            
            _webRtcClient.OnCameraSdp += (_, e) =>
                _hubConnection.SendAsync("CameraAnswer", e.Item1, e.Item2);
            
            _webRtcClient.OnDesktopSdp += (_, e) =>
                _hubConnection.SendAsync("DesktopAnswer", e.Item1, e.Item2);
            
            _webRtcClient.OnDesktopIceCandidate += (_, e) =>
                _hubConnection.SendAsync("DesktopIceCandidate", e.Item1, e.Item2);
            
            _webRtcClient.OnCameraIceCandidate += (_, e) =>
                _hubConnection.SendAsync("CameraIceCandidate", e.Item1, e.Item2);

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

            _hubConnection.On<string, string, string>("ReceiveMessage",
                async (testTaker, messageType, message) =>
                {
                    getExamTakerVideoCard(testTaker)?.AddMessage(messageType, message);
                    await Notification.Open(new NotificationConfig()
                    {
                        Message = (messageType == "warning" ? "Warning from " : "Message from ") + testTaker,
                        NotificationType = messageType == "warning" ? NotificationType.Warning : NotificationType.Info,
                        Description = message
                    });
                });

            _hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedDesktopOffer",
                async (taker, sdp) =>
                    await _webRtcClient.OnReceivedDesktopSdp(taker, sdp));

            _hubConnection.On<string, RTCIceCandidate>("ReceivedDesktopIceCandidate",
                async (taker, candidate) =>
                    await _webRtcClient.OnReceivedDesktopIceCandidate(taker, candidate));
            
            _hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedCameraOffer",
                async (taker, sdp) =>
                    await _webRtcClient.OnReceivedCameraSdp(taker, sdp));
            
            _hubConnection.On<string, RTCIceCandidate>("ReceivedCameraIceCandidate",
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
            _banTakerName = testTaker;
            _banModalVisible = true;
        }

        private async Task BanTestTakerConfirm()
        {
            var res = await ExamServices.BanExamTaker(_examId, _banTakerName, _banReason);

            if (res == ErrorCodes.Success)
            {
                await Modal.SuccessAsync(new ConfirmOptions()
                {
                    Content = $"Exam taker {_banTakerName} have been banned"
                });

                getExamTakerVideoCard(_banTakerName).Banned = true;
            }
            else
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot ban exam taker",
                    Content = ErrorCodes.MessageMap[res]
                });
            }

            _banTakerName = null;
            _banModalVisible = false;
        }

        private void BanTestTakerCancel()
        {
            _banTakerName = null;
            _banModalVisible = false;
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