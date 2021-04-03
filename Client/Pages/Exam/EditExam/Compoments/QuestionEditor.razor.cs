﻿using System;
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
        public IExamServices ExamServices { get; set; }

        [Parameter] 
        public int ExamId { get; set; }

        [Parameter] 
        public int QuestionNum { get; set; }


        private bool _editChoice = false;

        private string _currentEditChoice = null;
        private HtmlEditor _questionTextEditor;
        private HtmlEditor _choiceEditor;

        private bool _choiceModalLoaded = false;

        private string _lastQuestionType = "";

        private BaseQuestion _question = new ChoiceQuestion()
        {
            Choices = new List<string>()
            {
                "Choice 1"
            }
        };

        protected override async Task OnInitializedAsync()
        {
            var (res, question) = await ExamServices.GetQuestion(ExamId, QuestionNum);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot load question " + QuestionNum,
                    Content = ErrorCodes.MessageMap[res]
                });
                
                return;
            }

            _question = question;
            StateHasChanged();
        }

        public async Task SaveQuestion()
        {
            var res = 1001; // await ExamServices.UpdateQuestion(ExamId, QuestionNum);
            
            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot save question " + QuestionNum,
                    Content = ErrorCodes.MessageMap[res]
                });
            }
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

            await _choiceEditor.LoadHtmlString("");
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

            await _choiceEditor.LoadHtmlString(_currentEditChoice);
        }

        private void OnRemoveChoice(string choice)
        {
            if (_question is ChoiceQuestion q)
            {
                q.Choices.Remove(choice);
            }

            StateHasChanged();
        }

        private void OnQuestionTypeChange()
        {
            // Added to prevent a bug, the bind event sometimes will be called again and again
            if (_question.QuestionType == _lastQuestionType)
                return;

            var questionText = _question.Question;
            var questionType = _question.QuestionType;
            
            if (questionType == "choice")
            {
                _question = new ChoiceQuestion();
                ((ChoiceQuestion) _question).Choices = new List<string>();
            }
            else if (questionType == "short_answer")
            {
                _question = new ShortAnswerQuestion();
            }

            _question.Question = questionText;
            _question.QuestionType = questionType;
            _lastQuestionType = questionType;
            
            StateHasChanged();
        }


        private async Task OnChoiceConfirmed()
        {
            var content = await _choiceEditor.GetHtmlString();
            if (_question is ChoiceQuestion q)
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
    }
}