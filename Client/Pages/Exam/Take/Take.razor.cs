using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
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
        
        [Inject]
        public IExamServices ExamServices { get; set; }
        
        [Parameter]
        public string ExamId { get; set; }

        private int _examId;
        private IList<BaseQuestion> _questions;

        private bool _examNotBegin = false;

        private ExamDetailsResponseModel _examDetails;
        protected override async Task OnInitializedAsync()
        {
            _examId = int.Parse(ExamId);
            var (res, details) = await ExamServices.GetExamDetails(_examId);
            if (res == ErrorCodes.Success)
            {
                _examDetails = details;
            }

            var (res2, questions) = await ExamServices.GetPaper(_examId);
            if (res2 == ErrorCodes.ExamNotBegin)
            {
                _examNotBegin = true;
            }
            else if (res2 == ErrorCodes.Success)
            {
                _questions = questions;
            }
        }
    }
}