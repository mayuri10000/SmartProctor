using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamTakerVideoCard
    {
        [Parameter]
        public string ExamTakerName { get; set; }

        private bool _showToolBar = false;
        private bool _showingDesktop = false;
        private bool _fullscreen = false;

        private bool _loading = true;
        
        private void OnMouseOver()
        {
            _showToolBar = true;
        }

        private void OnMouseOut()
        {
            _showToolBar = false;
        }

        private async Task BanUser()
        {
            
        }

        private void ToggleFullscreen()
        {
            _fullscreen = !_fullscreen;
        }
    }
}