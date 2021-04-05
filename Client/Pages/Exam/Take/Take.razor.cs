using Microsoft.AspNetCore.Components;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class Take
    {
        [Parameter]
        public string ExamId { get; set; }
    }
}