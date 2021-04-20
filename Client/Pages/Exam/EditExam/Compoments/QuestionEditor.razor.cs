using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Questions;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class QuestionEditor
    {
        [Inject]
        public ModalService Modal { get; set; }
        
        [Inject]
        public MessageService Msg { get; set; }
        
        [Inject]
        public IExamServices ExamServices { get; set; }

        [Parameter]
        public int QuestionNum { get; set; }

        [Parameter] 
        public BaseQuestion Question { get; set; }
        
        [Parameter]
        public EventCallback<BaseQuestion> QuestionUpdated { get; set; }
        
        [Parameter] 
        public EventCallback OnRemoveQuestion { get; set; }


        private bool _editChoice = false;

        private string _currentEditChoice = null;
        private HtmlEditor _questionTextEditor;
        private string _choiceEditor;

        private bool _choiceModalLoaded = false;
        private bool _parameterSetState = false;

        private string _lastQuestionType = "";

        public async Task SaveQuestion()
        {
            Question.Question = await _questionTextEditor.GetHtmlString();
        }

        private async Task OnAddChoice()
        {
            _currentEditChoice = null;
            _editChoice = true;
            if (!_choiceModalLoaded)
            {
                // Ant design's modal loading is quite LAZY, it won't load until
                // the code tells it to do so, there's a delay from its visibility
                // being set true to when its content is loaded
                await Task.Delay(300);
                _choiceModalLoaded = true;
            }

            _choiceEditor = "";
        }

        private async Task OnEditChoice(string choice)
        {
            _currentEditChoice = choice;
            _editChoice = true;
            if (!_choiceModalLoaded)
            {
                await Task.Delay(300);
                _choiceModalLoaded = true;
            }

            _choiceEditor = _currentEditChoice;
        }

        private void OnRemoveChoice(string choice)
        {
            if (Question is ChoiceQuestion q)
            {
                q.Choices.Remove(choice);
            }

            StateHasChanged();
        }

        private void OnQuestionTypeChange()
        {
            // Added to prevent a bug, the bind event sometimes will be called again and again
            if (Question.QuestionType == _lastQuestionType)
                return;

            if (_parameterSetState)
            {
                _parameterSetState = false;
                return;
            }
            
            Console.WriteLine("OnQuestionTypeChange");

            var questionText = Question.Question;
            var questionType = Question.QuestionType;
            
            if (questionType == "choice")
            {
                Question = new ChoiceQuestion();
                ((ChoiceQuestion) Question).Choices = new List<string>();
            }
            else if (questionType == "short_answer")
            {
                Question = new ShortAnswerQuestion();
            }

            Question.Question = questionText;
            Question.QuestionType = questionType;
            _lastQuestionType = questionType;

            QuestionUpdated.InvokeAsync(Question);
            StateHasChanged();
        }


        private async Task OnChoiceConfirmed()
        {
            var content = _choiceEditor;
            if (Question is ChoiceQuestion q)
            {
                if (q.Choices.Contains(content) && _currentEditChoice != content)
                {
                    await Modal.ErrorAsync(new ConfirmOptions()
                    {
                        Title = "Duplicate options",
                        Content = "Please make sure that each choice for the question are different."
                    });
                }
                else if (_currentEditChoice != null && _currentEditChoice != content)
                {
                    var index = q.Choices.IndexOf(_currentEditChoice);
                    q.Choices[index] = content;
                }
                else if (_currentEditChoice == null)
                {
                    q.Choices.Add(content);
                }
            }

            _editChoice = false;
            StateHasChanged();
        }

        private void OnChoiceCancel()
        {
            _editChoice = false;
            _currentEditChoice = null;
        }

        protected override void OnParametersSet()
        {
            _parameterSetState = true;
        }
    }
}