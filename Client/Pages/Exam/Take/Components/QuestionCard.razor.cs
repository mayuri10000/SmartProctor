using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Answers;
using SmartProctor.Shared.Questions;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class QuestionCard
    {
        [Inject] 
        public IExamServices ExamServices { get; set; }
    
        [Inject]
        public ModalService Modal { get; set; }
        
        [Inject]
        public MessageService Message { get; set; }
    
        [Parameter]
        public int ExamId { get; set; }
    
        [Parameter]
        public int QuestionNum { get; set; }

        private BaseQuestion _question = new BaseQuestion()
        {
            Question = "Loading question, please wait..."
        };

        private BaseAnswer _answer;

        private HtmlEditor _answerRichTextEditor;
        private string _answerString;
        private int _choiceSingle;
        private bool[] _choiceChecked;

        protected override async Task OnInitializedAsync()
        {
            var (res, question) = await ExamServices.GetQuestion(ExamId, QuestionNum);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = $"Cannot load question {QuestionNum}",
                    Content = ErrorCodes.MessageMap[res]
                });
                return;
            }

            _question = question;

            if (question.QuestionType == "choice")
            {
                _answer = new ChoiceAnswer();
                _choiceChecked = new bool[((ChoiceQuestion)question).Choices.Count];
            }
            else if (question.QuestionType == "short_answer")
            {
                _answer = new ShortAnswer();
            }

            _answer.Type = question.QuestionType;
            
            StateHasChanged();
        }

        private async Task OnSubmitAnswer()
        {
            if (_question is ShortAnswerQuestion saq)
            {
                if (saq.RichText && _answerRichTextEditor != null)
                {
                    ((ShortAnswer) _answer).Answer = await _answerRichTextEditor.GetHtmlString();
                }
                else
                {
                    ((ShortAnswer) _answer).Answer = _answerString;
                }
            } 
            else if (_question is ChoiceQuestion cq)
            {
                if (cq.MultiChoice)
                {
                    for (var i = 0; i < _choiceChecked.Length; i++)
                    {
                        if (_choiceChecked[i]) 
                            ((ChoiceAnswer) _answer).Choices.Add(i);
                    }
                }
                else
                {
                    ((ChoiceAnswer) _answer).Choices.Add(_choiceSingle);
                }
            }
            
            var res = await ExamServices.SubmitAnswer(ExamId, QuestionNum, _answer);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = $"Cannot submit answer for question {QuestionNum}",
                    Content = ErrorCodes.MessageMap[res]
                });
            }
            else
            {
                await Message.Success($"Answer for question {QuestionNum} submitted");
            }
        }
    }
}