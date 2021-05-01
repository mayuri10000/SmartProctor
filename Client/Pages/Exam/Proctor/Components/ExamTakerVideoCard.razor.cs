using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Shared.Responses;

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

        [Parameter] public bool DesktopLoading { get; set; } = true;

        [Parameter] public bool CameraLoading { get; set; } = true;
        
        [Parameter]
        public bool Banned { get; set; }
        
        [Parameter]
        public int CardsEachRow { get; set; }

        private bool _showToolBar = false;
        private bool _showingDesktop = false;
        private bool _fullscreen = false;
        
        private bool _haveNewMessage = false;
        
        private List<EventItem> _messages = new List<EventItem>();
        private bool _messageVisible = false;

        public void AddMessage(EventItem eventItem)
        {
            _messages.Add(eventItem);
            _haveNewMessage = true;
        }

        public void AddOldMessage(EventItem eventItem)
        {
            _messages.Add(eventItem);
        }
        
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
                await OnToggleDesktop.InvokeAsync();
            }
            else
            {
                await OnToggleCamera.InvokeAsync();
            }
        }

        private void OpenMessage()
        {
            _haveNewMessage = false;
            _messageVisible = true;
        }

        private void CloseMessage()
        {
            _messageVisible = false;
        }
    }
}