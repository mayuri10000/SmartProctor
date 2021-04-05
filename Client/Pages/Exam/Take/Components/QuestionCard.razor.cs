using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Components;
using SmartProctor.Client.Services;
using SmartProctor.Shared.Questions;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class QuestionCard
    {
        [Inject] 
        public IExamServices ExamServices { get; set; }
    
        [Inject]
        public ModalService Modal { get; set; }
    
        [Parameter]
        public int ExamId { get; set; }
    
        [Parameter]
        public int QuestionNum { get; set; }

        private BaseQuestion _question = new BaseQuestion()
        {
            Question = "Loading question, please wait..."
        };

        private HtmlEditor _answerRichTextEditor;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
    }
}