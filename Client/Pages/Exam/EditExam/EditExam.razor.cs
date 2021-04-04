using System;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
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
        
        private UpdateExamDetailsRequestModel _updateExamDetailsModel = new UpdateExamDetailsRequestModel();

        private int _examId;
        private int _questionCount;

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

            _questionCount = details.QuestionCount;
            _questionEditors = new QuestionEditor[_questionCount];
            
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
            // TODO: Reload basic information
        }

        private void OnAddQuestion()
        {
            _questionCount++;
            Array.Resize(ref _questionEditors, _questionCount);
            StateHasChanged();
        }

        private async Task OnSavePaper()
        {
            await HandleUpdateBasicInfo();
            foreach (var e in _questionEditors)
            {
                await e.SaveQuestion();
            }
        }
    }
}