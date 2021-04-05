using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntDesign;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ListMine
    {
        
    private string _searchKeyword;
        private string _selectedExamState = "all";

        private IList<ExamDetails> _examList;
        private IList<ExamDetails> _filteredExamList;


        protected override async Task OnInitializedAsync()
        {
            await LoadExamList();
            _filteredExamList = _examList;
        }

        private void OnSearchExam()
        {
            var q = _searchKeyword != null ? _examList.Where(x => x.Name.Contains(_searchKeyword)) : _examList;
            if (_selectedExamState == "pending")
            {
                q = q.Where(x => x.StartTime > DateTime.Now);
            }
            else if (_selectedExamState == "ended")
            {
                q = q.Where(x => x.StartTime.AddSeconds(x.Duration) < DateTime.Now);
            }

            _filteredExamList = q.ToList();
        }

        private void EnterExam(int examId)
        {
            NavManager.NavigateTo("/Exam/Details/" + examId);
        }

        private async Task LoadExamList()
        {
            var (code, res) = await ExamServices.GetExams(3);

            if (code == ErrorCodes.Success)
            {
                _examList = res;

                return;
            }

            if (code == ErrorCodes.NotLoggedIn)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Please login first",
                });
                NavManager.NavigateTo("/User/Login");
            }
            else
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Failed to get exam list",
                    Content = ErrorCodes.MessageMap[code]
                });
            }
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