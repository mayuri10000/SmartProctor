using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.SignalR.Client;
using SmartProctor.Client.WebRTCInterop;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamPage
    {
        [Parameter]
        public string ExamId { get; set; }

        private int currentQuestionNum = 1;

        private ExamDetailsResponseModel examDetails;
        private WebRTCClientTaker _webRtcClient;
        
        private HubConnection hubConnection;

        private bool localDesktopVideoLoaded = false;
        private bool localCameraVideoLoded = false;

        protected override async Task OnInitializedAsync()
        {
            /*
            var result = await Http.GetFromJsonAsync<BaseResponseModel>("api/exam/Attempt/" + ExamId);

            if (result.Code == 1000)
            {
                Modal.Error(new ConfirmOptions()
                {
                    Title = "You must login first",
                });
                NavManager.NavigateTo("/User/Login");
            }
            else if (result.Code != 0)
            {
                Modal.Error(new ConfirmOptions()
                {
                    Title = "Enter test failed",
                    Content = result.Message
                });
                NavManager.NavigateTo("/");
            }
            else
            {
                var details = await Http.GetFromJsonAsync<ExamDetailsResponseModel>("api/exam/ExamDetails/" + ExamId);

                if (details.Code == 0)
                {
                    examDetails = details;
                }

                await SetupSignalRClientAsync();
                StateHasChanged();
            }
            */
            
            _webRtcClient = new WebRTCClientTaker(JsRuntime, new [] { "1" });
            _webRtcClient.OnProctorSdp += (sender, tuple) =>
            {
                Modal.Success(new ConfirmOptions()
                {
                    Title = tuple.Item2.Type,
                    Content = tuple.Item2.Sdp
                });
            };
            await _webRtcClient.StartStreamingDesktop();

        }

        private async Task OnDesktopVideoVisibleChange(bool visible)
        {
            if (visible && !localDesktopVideoLoaded)
            {
                await _webRtcClient.SetDesktopVideoElement("local-desktop");
            }
        }

        private async Task OnCameraVideoVisibleChange(bool visible)
        {
            if (visible && !localCameraVideoLoded)
            {
                await _webRtcClient.SetCameraVideoElement("local-camera");
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
            
            RenderFragment rf = builder =>
            {
            };
        }

        private async Task SetupSignalRClientAsync()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/hub"))
                .Build();

            hubConnection.On<string>("ReceiveMessage", OnReceiveMessage);
            
            await hubConnection.StartAsync();
        }

        private void OnReceiveMessage(string message)
        {
            // TODO: Process and display message
        }

        private async Task SendMessage(string message)
        {
            await hubConnection.SendAsync("SendMessage", message);
        }
    }
}