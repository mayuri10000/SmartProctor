using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntDesign;
using BrowserInterop;
using BrowserInterop.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using SmartProctor.Client.Components;
using SmartProctor.Client.Interops;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared;
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Responses;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class Take
    {
        private RenderFragment _logo = _ =>
        {
            // No logo, empty
        };
        
        [Inject]
        public IExamServices ExamServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        [Inject]
        public NotificationService Notification { get; set; }
        
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Parameter]
        public string ExamId { get; set; }

        private int _examId;
        private IList<BaseQuestion> _questions;
        
        private WebRTCClientTaker _webRtcClient;
        private HubConnection _hubConnection;
        private IList<string> _proctors;
        
        private bool _localDesktopVideoLoaded = false;
        private bool _localCameraVideoLoaded = false;
        private bool _inPrepare = false;
        private bool _pageLeft = false;

        private bool _inReshare = false;

        private TestPrepareModal _testPrepareModal;
        private ChatDrawer _chatDrawer;

        private WindowInterop _window;

        private bool _examNotBegin = false;

        private ExamDetailsResponseModel _examDetails;

        private List<EventItem> _messages = new List<EventItem>();
        
        /// <summary>
        /// Initialize method of this page, will be called by runtime
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            if (!int.TryParse(ExamId, out _examId))
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot enter exam",
                    Content = "Invalid examId"
                });
                return;
            }

            if (await Attempt())
            {
                _inPrepare = true;
                await GetExamDetails();
                await GetProctors();
                await SetupSignalRClient();
                await SetupBlurMonitor();
                await GetQuestions();
                SetupWebRTCClient();
                StateHasChanged();
                await Task.Delay(500); // Delay to give enough time for the page to be loaded
                await GetEvents();
            }
        }

        #region Initialize methods to obtain information

        private async Task GetProctors()
        {
            var (ret, proctors) = await ExamServices.GetProctors(_examId);
            if (ret == ErrorCodes.Success)
            {
                _proctors = proctors.Select(x => x.Id).ToList();
            }
        }
        
        private async Task GetExamDetails()
        {
            var (res, details) = await ExamServices.GetExamDetails(_examId);
            if (res == ErrorCodes.Success)
            {
                _examDetails = details;
            }
        }

        private async Task GetEvents()
        {
            var (res, proctorEvents) = await ExamServices.GetEvents(_examId, Consts.MessageTypeProctor);
            if (res == ErrorCodes.Success)
            {
                _messages.AddRange(proctorEvents);
            }

            var (res2, takerEvents) = await ExamServices.GetEvents(_examId, Consts.MessageTypeTaker);
            if (res == ErrorCodes.Success)
            {
                _messages.AddRange(takerEvents);
            }
            
            _messages.Sort((a, b) => DateTime.Compare(a.Time, b.Time));
        }
        
        private async Task GetQuestions()
        {
            var (res, questions) = await ExamServices.GetPaper(_examId);
            if (res == ErrorCodes.ExamNotBegin)
            {
                // If the exam not begin, display the count down
                _examNotBegin = true;
            }
            else if (res == ErrorCodes.Success)
            {
                _examNotBegin = false;
                _questions = questions;
            }
            StateHasChanged();
        }

        #endregion

        #region WebRTC

        private void SetupWebRTCClient()
        {
            _webRtcClient = new WebRTCClientTaker(JsRuntime, _proctors.ToArray());
            
            _webRtcClient.OnDesktopSdp += (_, e) =>
            {
                _hubConnection.SendAsync("DesktopOffer", e.Item1, e.Item2);
            };

            _webRtcClient.OnDesktopIceCandidate += (_, e) =>
            {
                _hubConnection.SendAsync("DesktopIceCandidate", e.Item1, e.Item2);
            };

            _webRtcClient.OnCameraSdp += (_, e) =>
            {
                _hubConnection.SendAsync("CameraOffer", e.Item1, e.Item2);
            };

            _webRtcClient.OnCameraIceCandidate += (_, e) =>
            {
                _hubConnection.SendAsync("CameraIceCandidate", e.Item1, e.Item2);
            };

            _webRtcClient.OnDesktopInactivated += async (_, __) =>
            {
                await OnReshareScreen();
            };
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
                await _webRtcClient.StartStreaming();
            }
        }
        
        private async Task OnGetCameraStream(string mjpegUrl)
        {
            await _webRtcClient.ObtainCameraStream(mjpegUrl);
            await _webRtcClient.SetCameraVideoElement("camera-video-dialog");
        }
        
        private async Task OnReshareScreen()
        {
            await ExamServices.SendEvent(_examId, 1, "The test taker's desktop capture was cancelled", null);
            _inReshare = true;
            await Modal.ErrorAsync(new ConfirmOptions()
            {
                Title = "Your desktop capture is interrupted",
                Content =
                    "You could probably clicked the 'Stop sharing' button, click 'OK' to re-initialize your screen capture"
            });
            while (_inReshare)
            {
                var streamId = await _webRtcClient.ObtainDesktopStream();
                if (streamId is "screen:0:0" or "Screen 1")
                {
                    if (_localDesktopVideoLoaded)
                        await _webRtcClient.SetDesktopVideoElement("local-desktop");
                    await _webRtcClient.StartStreaming();
                    _inReshare = false;
                }
                else
                {
                    await Modal.ErrorAsync(new ConfirmOptions()
                    {
                        Title = "Your screen capture is not valid",
                        Content =
                            "Please make sure you have shared your entire screen instead of a window, and make sure" +
                            "that you have only one monitor."
                    });
                }
            }
        }
        
        private async Task OnReshareScreenFinish()
        {
            if (_localDesktopVideoLoaded)
            {
                await _webRtcClient.SetDesktopVideoElement("local-desktop");
            }
            _inReshare = false;
        }
        #endregion

        #region Screen blur monitor

        /// <summary>
        /// Sets up the callback that called when the page is blurred.
        /// Blur can occur if the browser tab was switched to background, or the browser window does not have focus
        /// </summary>
        private async Task SetupBlurMonitor()
        {
            _window = await JsRuntime.Window();
            await _window.OnBlur(OnBlur);
        }

        /// <summary>
        /// Callback that called when the page is blurred. This will send a warning to the proctor and a alert will
        /// be displayed
        /// </summary>
        private async ValueTask OnBlur()
        {
            if (!_inPrepare && !_pageLeft && !_inReshare)
            {
                _pageLeft = true;
                await _window.Focus();
                await ExamServices.SendEvent(_examId, 1, "The exam taker left the exam page", null)
                    .ConfigureAwait(false);
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "DO NOT LEAVE THE EXAM PAGE!",
                    Content = "Your screen monitored by the proctors."
                }).ConfigureAwait(false);
                _pageLeft = false;
            }
        }

        #endregion
        
        /// <summary>
        /// Sets up the event callbacks of the SignalR client and starts the client
        /// </summary>
        private async Task SetupSignalRClient()
        {
            // Connect to the SignalR hub
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/hub"))
                .Build();

            // Proctor message handler
            _hubConnection.On<EventItem>("ReceivedMessage",
                async message =>
                {
                    // Add the new message and show a notification
                    _messages.Add(message);
                    _chatDrawer.IncrementMessage();
                    await Notification.Info(new NotificationConfig()
                    {
                        Message = "Message from proctor",
                        Description = message.Message
                    });
                });

            // Callback of SDP answer received from proctor, should be handled by the WebRTC client
            _hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedDesktopAnswer",
                async (proctor, sdp) =>
                {
                    await _webRtcClient.ReceivedDesktopAnswerSDP(proctor, sdp);
                });

            // 
            _hubConnection.On<string, RTCIceCandidate>("ReceivedDesktopIceCandidate",
                async (proctor, candidate) =>
                {
                    await _webRtcClient.ReceivedDesktopIceCandidate(proctor, candidate);
                });
            _hubConnection.On<string, RTCSessionDescriptionInit>("ReceivedCameraAnswer",
                async (proctor, sdp) =>
                {
                    await _webRtcClient.ReceivedCameraAnswerSDP(proctor, sdp);
                });

            _hubConnection.On<string, RTCIceCandidate>("ReceivedCameraIceCandidate",
                async (proctor, candidate) =>
                {
                    await _webRtcClient.ReceivedCameraIceCandidate(proctor, candidate);
                });
            
            // Callback called when a new proctor connected, should reconnect the WebRTC stream
            _hubConnection.On<string>("ProctorConnected",
                async proctor =>
                {
                    await _webRtcClient.ReconnectToProctor(proctor);
                });
            
            // Callback called when the exam taker was banned, should display a warning and exit the exam
            _hubConnection.On<string>("ExamTakerBanned",
                async reason =>
                {
                    await Modal.ErrorAsync(new ConfirmOptions()
                    {
                        Title = "You have been banned by the proctor",
                        Content = "Reason: " + reason
                    });
                    await ExitExam();
                });

            await _hubConnection.StartAsync();
        }

        private async Task SendMessage(string message)
        {
            var res = await ExamServices.SendEvent(_examId, Consts.MessageTypeTaker, message, null);
            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Failed to send message",
                    Content = ErrorCodes.MessageMap[res]
                });
            }
            else
            {
                _messages.Add(new EventItem()
                {
                    Message = message, Sender = "Me", Time = DateTime.Now, Type = Consts.MessageTypeTaker
                });
            }
        }
        
        /// <summary>
        /// Attempts to enter the exam. Display errors if failed
        /// </summary>
        /// <returns>True if the exam is eligible for current user to take</returns>
        private async Task<bool> Attempt()
        {
            var (result, banReason) = await ExamServices.Attempt(_examId);

            if (result == ErrorCodes.NotLoggedIn)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "You must login first",
                });
                NavManager.NavigateTo("/User/Login", true);
                return false;
            }
            else if (banReason != null)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "You were banned from this exam",
                    Content = "Reason:" + banReason,
                });
                NavManager.NavigateTo("/", true);
                return false;
            }
            else if (result != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Enter test failed",
                    Content = ErrorCodes.MessageMap[result]
                });
                NavManager.NavigateTo("/", true);
                return false;
            }

            return true;
        }
        
        private void OnPrepareFinish()
        {
            _inPrepare = false;
        }

        private async Task ExitExam()
        {
            await _hubConnection.SendAsync("ExamEnded");
            await _hubConnection.StopAsync();
            NavManager.NavigateTo("/", true);
        }
        
        private string ConvertExamDuration(int secs)
        {
            var hours = secs / 3600;
            var minutes = (secs - hours * 3600) / 60;
            var seconds = secs - minutes * 60 - hours * 3600;

            var sb = new StringBuilder();
            if (hours > 0)
            {
                sb.Append(hours);
                sb.Append(" Hours ");
            }

            if (minutes > 0)
            {
                sb.Append(minutes);
                sb.Append(" Minutes ");
            }

            if (seconds > 0)
            {
                sb.Append(seconds);
                sb.Append(" Seconds ");
            }

            return sb.ToString();
        }
    }
}