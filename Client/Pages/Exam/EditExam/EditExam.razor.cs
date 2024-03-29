﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Requests;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class EditExam
    {
        [Parameter]
        public string ExamId { get; set; }
        
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Inject]
        public IExamServices ExamServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        [Inject]
        public MessageService Message { get; set; }
        
        #region Layout parameters
        private readonly FormItemLayout _formItemLayout = new FormItemLayout
        {
            LabelCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty {Span = 24},
                Sm = new EmbeddedProperty {Span = 7},
            },

            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty {Span = 24},
                Sm = new EmbeddedProperty {Span = 12},
                Md = new EmbeddedProperty {Span = 10},
            }
        };

        private readonly FormItemLayout _submitFormLayout = new FormItemLayout
        {
            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24, Offset = 0},
                Sm = new EmbeddedProperty { Span = 10, Offset = 7},
            }
        };
        #endregion
        
        private UpdateExamDetailsRequestModel _updateExamDetailsModel = new UpdateExamDetailsRequestModel()
        {
            // Added to solve a bug of Ant Design Blazor's DatePicker component.
            // The component will run into a problem if the initial value is invalid
            // even if a valid value was given later
            Duration = new DateTime(1999, 4, 27, 2, 0, 0),
            StartTime = DateTime.Now
        };

        private int _examId;
        private int _questionCount;

        private IList<BaseQuestion> _questions;
        private QuestionEditor[] _questionEditors;

        protected override async Task OnInitializedAsync()
        {
            if (!int.TryParse(ExamId, out _examId))
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot obtain exam information",
                    Content = "Invalid exam ID"
                });
                
                return;
            }

            var (res, details) = await ExamServices.GetExamDetails(_examId);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot obtain exam information",
                    Content = ErrorCodes.MessageMap[res]
                });
                
                if (res == ErrorCodes.NotLoggedIn)
                    NavManager.NavigateTo("/User/Login");
                
                return;
            }

            var secs = details.Duration;
            var hours = secs / 3600;
            var minutes = (secs - hours * 3600) / 60;
            var seconds = secs - minutes * 60 - hours * 3600;
            
            _updateExamDetailsModel.Id = _examId;
            _updateExamDetailsModel.Name = details.Name;
            _updateExamDetailsModel.Description = details.Description;
            _updateExamDetailsModel.StartTime = details.StartTime;
            _updateExamDetailsModel.Duration = new DateTime(1999, 04, 27, hours, minutes, seconds);
            _updateExamDetailsModel.OpenBook = details.OpenBook;
            _updateExamDetailsModel.MaximumTakersNum = details.MaxTakers;

            var (res2, questions) = await ExamServices.GetPaper(_examId);
            if (res2 != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot obtain exam paper",
                    Content = ErrorCodes.MessageMap[res2]
                });
                
                return;
            }

            _questions = questions;
            _questionEditors = new QuestionEditor[_questions.Count];
            StateHasChanged();
        }

        private async Task HandleUpdateBasicInfo()
        {
            var res = await ExamServices.UpdateExamDetails(_updateExamDetailsModel);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot update exam information",
                    Content = ErrorCodes.MessageMap[res]
                });
                
                return;
            }

            await Message.Success("Exam information updated");
            var (res2, details) = await ExamServices.GetExamDetails(_examId);

            if (res2 == ErrorCodes.Success)
            {
                var secs = details.Duration;
                var hours = secs / 3600;
                var minutes = (secs - hours * 3600) / 60;
                var seconds = secs - minutes * 60 - hours * 3600;
                
                _updateExamDetailsModel.Name = details.Name;
                _updateExamDetailsModel.Description = details.Description;
                _updateExamDetailsModel.StartTime = details.StartTime;
                _updateExamDetailsModel.Duration = new DateTime(1999, 04, 27, 10, minutes, seconds);
                _updateExamDetailsModel.OpenBook = details.OpenBook;
                _updateExamDetailsModel.MaximumTakersNum = details.MaxTakers;
            }
        }

        private void OnAddQuestion()
        {
            _questions.Add(new ChoiceQuestion()
            {
                QuestionType = "choice",
                Choices = new List<string>()
            });
            Array.Resize(ref _questionEditors, _questions.Count);
            StateHasChanged();
        }

        private async Task OnSavePaper()
        {
            await HandleUpdateBasicInfo();
            foreach (var e in _questionEditors)
            {
                await e.SaveQuestion();
            }

            var res = await ExamServices.UpdatePaper(_examId, _questions);
            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot save exam paper",
                    Content = ErrorCodes.MessageMap[res]
                });
                
                return;
            }

            await Message.Success("Exam paper saved");
        }

        private async Task RemoveQuestion(int index)
        {
            foreach (var e in _questionEditors)
            {
                await e.SaveQuestion();
            }

            _questions.RemoveAt(index);
            Array.Resize(ref _questionEditors, _questions.Count);
            StateHasChanged();
        }

        private void UpdateQuestion(int index, BaseQuestion question)
        {
            _questions[index] = question;
        }
    }
}