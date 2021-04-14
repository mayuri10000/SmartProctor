using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AntDesign;
using AntDesign.Pro.Layout;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class Details
    {
        [Parameter] public string ExamId { get; set; }
        
        [Inject] public IExamServices ExamServices { get; set; }
        
        [Inject] public ModalService Modal { get; set; }
        
        [Inject] public NavigationManager NavManager { get; set; }

        private ExamDetailsResponseModel _model = new ExamDetailsResponseModel();
        private IList<UserBasicInfo> _takers = new List<UserBasicInfo>();
        private IList<UserBasicInfo> _proctors = new List<UserBasicInfo>();
        private int _examId;
        
        private readonly IList<TabPaneItem> _tabList = new List<TabPaneItem>
        {
            new TabPaneItem {Key = "questions", Tab = "Questions"},
            new TabPaneItem {Key = "takers", Tab = "Exam takers"},
            new TabPaneItem {Key = "proctors", Tab = "Proctors"}
        };

        private string _currentTab = "takers";

        protected override async Task OnInitializedAsync()
        {
            if (!int.TryParse(ExamId, out _examId))
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Invalid exam ID"
                });
                return;
            }

            var (res, details) = await ExamServices.GetExamDetails(_examId);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Failed to obtain exam information",
                    Content = ErrorCodes.MessageMap[res]
                });
                return;
            }

            _model = details;

            var (res2, takers) = await ExamServices.GetTestTakers(_examId);
            if (res2 != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Failed to obtain list of exam takers",
                    Content = ErrorCodes.MessageMap[res2]
                });
                return;
            }

            _takers = takers;
            
            var (res3, proctors) = await ExamServices.GetProctors(_examId);
            if (res3 != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Failed to obtain list of exam proctors",
                    Content = ErrorCodes.MessageMap[res2]
                });
                return;
            }

            _proctors = proctors;
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

        private void ToEdit()
        {
            NavManager.NavigateTo("/Exam/Edit/" + ExamId);
        }

        private void OnTabChange(string tab)
        {
            _currentTab = tab;
        }
    }
}