using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Answers;
using SmartProctor.Shared.Questions;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class Result
    {
        [Inject]
        public IExamServices ExamServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        [Parameter]
        public string ExamId { get; set; }
        
        [Parameter]
        public string UserId { get; set; }

        private int _examId;
        private ExamDetailsResponseModel _examDetails;
        private IList<BaseQuestion> _questions = new List<BaseQuestion>();

        protected override async Task OnInitializedAsync()
        {
            if (!int.TryParse(ExamId, out _examId))
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot get exam result",
                    Content = "Invalid examId"
                });
                return;
            }

            var (r, details) = await ExamServices.GetExamDetails(_examId);

            if (r != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot get exam result",
                    Content = ErrorCodes.MessageMap[r]
                });
                return;
            }

            _examDetails = details;

            var (res, questions) = await ExamServices.GetPaper(_examId);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot get exam result",
                    Content = ErrorCodes.MessageMap[res]
                });
                return;
            }

            _questions = questions;
            StateHasChanged();
        }
        
        private string ConvertExamDuration(int secs)
        {
            var hours = secs / 3600;
            var minutes = (secs - hours * 3600) / 60;
            var seconds = secs - minutes * 60 - hours * 3600;

            var sb = new StringBuilder();
            if (hours > 0)
            {
                sb.Append(hours);
                sb.Append(" Hours ");
            }

            if (minutes > 0)
            {
                sb.Append(minutes);
                sb.Append(" Minutes ");
            }

            if (seconds > 0)
            {
                sb.Append(seconds);
                sb.Append(" Seconds ");
            }

            return sb.ToString();
        }
    }
}