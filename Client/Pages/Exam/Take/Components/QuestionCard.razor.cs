using System;
using System.Collections.Generic;
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
        public BaseQuestion Question { get; set; }
        
        [Parameter]
        public int QuestionNum { get; set; }

        private BaseAnswer _answer;

        private HtmlEditor _answerRichTextEditor;
        private string _answerString;
        private int _choiceSingle;
        private bool[] _choiceChecked;

        private bool _initialized = false;

        protected override async Task OnInitializedAsync()
        {
            var (res2, answer, _) = await ExamServices.GetAnswer("", ExamId, QuestionNum);
            if (res2 == ErrorCodes.Success)
            {
                _answer = answer;
                if (answer is ChoiceAnswer choiceAnswer && Question is ChoiceQuestion choiceQuestion)
                {
                    if (choiceQuestion.MultiChoice)
                    {
                        _choiceChecked = new bool[choiceQuestion.Choices.Count];
                        foreach (var i in choiceAnswer.Choices)
                        {
                            _choiceChecked[i] = true;
                        }
                    }
                    else
                    {
                        _choiceSingle = choiceAnswer.Choices[0];
                    }
                }
                else if (answer is ShortAnswer shortAnswer && Question is ShortAnswerQuestion shortAnswerQuestion)
                {
                    _answerString = shortAnswer.Answer;
                }
                else
                {
                    if (Question is ChoiceQuestion)
                    {
                        _answer = new ChoiceAnswer();
                        _choiceChecked = new bool[((ChoiceQuestion)Question).Choices.Count];
                    }
                    else if (Question is ShortAnswerQuestion)
                    {
                        _answer = new ShortAnswer();
                    }

                    _answer.Type = Question.QuestionType;
                }
            }

            _initialized = true;
            StateHasChanged();
        }

        private async Task OnSubmitAnswer()
        {
            if (Question is ShortAnswerQuestion saq)
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
            else if (Question is ChoiceQuestion cq)
            {
                ((ChoiceAnswer) _answer).Choices = new List<int>();
                if (cq.MultiChoice)
                {
                    for (var i = 0; i < _choiceChecked.Length; i++)
                    {
                        if (_choiceChecked[i])
                        {
                            ((ChoiceAnswer) _answer).Choices.Add(i);
                        }
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

        private void OnRadioChange(int a)
        {
            OnSubmitAnswer();
        }
    }
}