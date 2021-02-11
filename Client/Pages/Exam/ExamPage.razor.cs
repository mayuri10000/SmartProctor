using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.SignalR.Client;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamPage
    {
        [Parameter]
        public string ExamId { get; set; }

        private int currentQuestionNum = 1;

        private ExamDetailsResponseModel examDetails;
        
        private HubConnection hubConnection;

        protected async override Task OnInitializedAsync()
        {
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
                .WithUrl(NavManager.ToAbsoluteUri("/chathub"))
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