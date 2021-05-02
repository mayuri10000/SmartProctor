using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntDesign;
using BrowserInterop;
using BrowserInterop.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using SmartProctor.Client.Interops;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared;
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

        private IDictionary<string, List<EventItem>> _takerMessages = new Dictionary<string, List<EventItem>>();
        private List<EventItem> _broadcastMessages = new List<EventItem>();
        private IList<EventItem> _currentMessages;
        private string _currentChatTaker = null;
        private bool _chatVisible = false;
        
        protected override async Task OnInitializedAsync()
        {
            _examId = int.Parse(ExamId);
            if (await Attempt())
            {
                await GetExamDetails();
                await GetExamTakers();
                await SetupSignalRClient();
                SetupWebRTC();
                await _hubConnection.SendAsync("ProctorJoin", ExamId);
                StateHasChanged();
                await Task.Delay(500);
                await GetEvents();
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

        private async Task GetEvents()
        {
            var (ret, warnings) = await ExamServices.GetEvents(_examId, Consts.MessageTypeWarning);

            if (ret == ErrorCodes.Success)
            {
                foreach (var ev in warnings)
                {
                    getExamTakerVideoCard(ev.Sender).AddOldMessage(ev);
                }
            }

            var (ret2, takerMessage) = await ExamServices.GetEvents(_examId, Consts.MessageTypeTaker);

            if (ret2 == ErrorCodes.Success)
            {
                foreach (var ev in takerMessage)
                {
                    if (!_takerMessages.ContainsKey(ev.Sender))
                    {
                        _takerMessages[ev.Sender] = new List<EventItem>();
                    }
                    
                    _takerMessages[ev.Sender].Add(ev);
                }
            }

            var (ret3, proctorMessage) = await ExamServices.GetEvents(_examId, Consts.MessageTypeProctor);

            if (ret3 == ErrorCodes.Success)
            {
                foreach (var ev in proctorMessage)
                {
                    if (ev.Receipt == null)
                    {
                        _broadcastMessages.Add(ev);
                    }
                    else
                    {
                        if (!_takerMessages.ContainsKey(ev.Receipt))
                        {
                            _takerMessages[ev.Receipt] = new List<EventItem>();
                        }
                        
                        _takerMessages[ev.Receipt].Add(ev);
                    }
                }
            }

            foreach (var key in _takerMessages.Keys)
            {
                _takerMessages[key].Sort((x, y) => DateTime.Compare(x.Time, y.Time));
            }
            
            _broadcastMessages.Sort((x, y) => DateTime.Compare(x.Time, y.Time));
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

            _webRtcClient.OnDesktopConnectionStateChange += (_, e) =>
                getExamTakerVideoCard(e.Item1).DesktopLoading = e.Item2 != "connected";
            
            _webRtcClient.OnCameraConnectionStateChange += (_, e) =>
                getExamTakerVideoCard(e.Item1).CameraLoading = e.Item2 != "connected";
        }

        private async Task NewMessage(EventItem message)
        {
            if (message.Type == Consts.MessageTypeWarning)
                getExamTakerVideoCard(message.Sender)?.AddWarningMessage(message);
            else
            {
                if (!_takerMessages.ContainsKey(message.Sender))
                {
                    _takerMessages[message.Sender] = new List<EventItem>();
                }
                
                _takerMessages[message.Sender].Add(message);
                getExamTakerVideoCard(message.Sender)?.SetNewMessage();
            }
            await Notification.Open(new NotificationConfig()
            {
                Message = (message.Type == Consts.MessageTypeWarning ? "Warning from " : "Message from ") + message.Sender,
                NotificationType = message.Type == Consts.MessageTypeWarning ? NotificationType.Warning : NotificationType.Info,
                Description = message.Message
            });
        }
        
        private async Task SetupSignalRClient()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/hub"))
                .Build();

            _hubConnection.On<EventItem>("ReceivedMessage",
                async (eventItem) =>
                    await NewMessage(eventItem));

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

        private void OnOpenMessage(string taker)
        {
            _currentChatTaker = taker;
            if (taker == null)
            {
                _currentMessages = _broadcastMessages;
            }
            else
            {
                if (!_takerMessages.ContainsKey(taker))
                {
                    _takerMessages[taker] = new List<EventItem>();
                }

                _currentMessages = _takerMessages[taker];
            }

            _chatVisible = true;
        }

        private async Task OnSendMessage(string message)
        {
            var ret = await ExamServices.SendEvent(_examId, Consts.MessageTypeProctor, message, _currentChatTaker);

            if (ret != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Failed to send message",
                    Content = ErrorCodes.MessageMap[ret]
                });
            }
            else
            {
                if (_currentChatTaker == null)
                {
                    _broadcastMessages.Add(new EventItem()
                    {
                        Message = message,
                        Receipt = _currentChatTaker,
                        Sender = "Me",
                        Time = DateTime.Now,
                        Type = Consts.MessageTypeProctor
                    });
                }
                else
                {
                    if (!_takerMessages.ContainsKey(_currentChatTaker))
                    {
                        _takerMessages[_currentChatTaker] = new List<EventItem>();
                    }

                    _takerMessages[_currentChatTaker].Add(new EventItem()
                    {
                        Message = message,
                        Receipt = _currentChatTaker,
                        Sender = "Me",
                        Time = DateTime.Now,
                        Type = Consts.MessageTypeProctor
                    });
                }
            }
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