using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class Take
    {
        private RenderFragment _logo = _ =>
        {
            // No logo, empty
        };
        
        [Parameter]
        public string ExamId { get; set; }

        private int _examId;
        private ExamDetails _examDetails = new ExamDetails();

        private int _questionNum;

        protected override async Task OnInitializedAsync()
        {
        }
    }
}