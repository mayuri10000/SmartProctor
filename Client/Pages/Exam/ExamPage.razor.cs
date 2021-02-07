using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamPage
    {
        [Parameter]
        public string ExamId { get; set; }

        private int currentQuestionNum = 1;

        private ExamDetailsResponseModel examDetails;

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
            }
            else
            {
                var details = await Http.GetFromJsonAsync<ExamDetailsResponseModel>("api/exam/ExamDetails/" + ExamId);

                if (details.Code == 0)
                {
                    examDetails = details;
                }
                
                StateHasChanged();
            }
        }

        public void OnFinish()
        {
            Modal.Warning(new ConfirmOptions()
            {
                Title = "Time's up",
            });
            
            RenderFragment rf = builder =>
            {
            };
        }
        
        
    }
}