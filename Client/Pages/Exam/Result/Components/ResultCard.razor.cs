using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Answers;
using SmartProctor.Shared.Questions;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ResultCard
    {
        [Inject]
        public IExamServices ExamServices { get; set; }

        [Parameter]
        public int ExamId { get; set; }
        
        [Parameter]
        public string UserId { get; set; }
        
        [Parameter]
        public BaseQuestion Question { get; set; }
        
        [Parameter]
        public int QuestionNum { get; set; }
        
        private BaseAnswer _answer;
        private DateTime _time;
        private string _displayText = "Loading answer, please wait...";

        protected override async Task OnInitializedAsync()
        {
            var (res, answer, time) = await ExamServices.GetAnswer(UserId, ExamId, QuestionNum);

            if (res == ErrorCodes.Success)
            {
                _answer = answer;
                _time = time;
            }
            else if (res == ErrorCodes.QuestionNotAnswered)
            {
                _displayText = "The exam taker did not answered this question";
            }
            else
            {
                _displayText = "Error fetching answer: " + ErrorCodes.MessageMap[res];
            }
        }
    }
}