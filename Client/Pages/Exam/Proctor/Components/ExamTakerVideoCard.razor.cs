using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ExamTakerVideoCard
    {
        [Parameter]
        public string ExamTakerName { get; set; }
        
        [Parameter]
        public EventCallback OnBanExamTaker { get; set; }
        
        [Parameter]
        public EventCallback OnToggleDesktop { get; set; }
        
        [Parameter]
        public EventCallback OnToggleCamera { get; set; }
        
        [Parameter]
        public bool Loading { get; set; }
        
        [Parameter]
        public bool Banned { get; set; }

        private bool _showToolBar = false;
        private bool _showingDesktop = false;
        private bool _fullscreen = false;
        
        private bool _haveNewMessage = false;
        
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
            await OnBanExamTaker.InvokeAsync();
        }

        private void ToggleFullscreen()
        {
            _fullscreen = !_fullscreen;
        }

        private async Task ToggleDesktopCamera()
        {
            if (_showingDesktop)
            {
                await OnToggleCamera.InvokeAsync();
            }
            else
            {
                await OnToggleDesktop.InvokeAsync();
            }

            _showingDesktop = !_showingDesktop;
        }
    }
}