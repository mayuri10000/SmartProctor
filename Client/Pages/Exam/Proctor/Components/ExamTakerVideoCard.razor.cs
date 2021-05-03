using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.Exam
{
    /// <summary>
    /// Component shows a video panel of a exam taker's desktop capture and
    /// camera capture. Should be placed in <see cref="Proctor"/> page.
    /// </summary>
    public partial class ExamTakerVideoCard
    {
        /// <summary>
        /// The exam taker's user ID
        /// </summary>
        [Parameter]
        public string ExamTakerName { get; set; }
        
        /// <summary>
        /// Callback when the "ban" button clicked
        /// </summary>
        [Parameter]
        public EventCallback OnBanExamTaker { get; set; }
        
        /// <summary>
        /// Callback when the video panel is switched to desktop viewing
        /// </summary>
        [Parameter]
        public EventCallback OnToggleDesktop { get; set; }
        
        /// <summary>
        /// Callback when the video panel is switched to camera viewing
        /// </summary>
        [Parameter]
        public EventCallback OnToggleCamera { get; set; }
        
        /// <summary>
        /// Callback when the "message" button is clicked
        /// </summary>
        [Parameter]
        public EventCallback OnOpenMessage { get; set; }

        /// <summary>
        /// Should display loading icon on the desktop video panel
        /// </summary>
        private bool _desktopLoading = true;

        /// <summary>
        /// Should display loading icon on the camera video panel
        /// </summary>
        private bool _cameraLoading = true;

        /// <summary>
        /// Should display ban icon on the video panel
        /// </summary>
        [Parameter]
        public bool Banned { get; set; }
        
        /// <summary>
        /// Number of this component in each rows in <see cref="Proctor"/> page.
        /// </summary>
        [Parameter]
        public int CardsEachRow { get; set; }
        
        // Shows the top and bottom bar or not, decided by whether the cursor is in the component
        private bool _showToolBar = false;   
        
        // Is the video panel showing desktop capture
        private bool _showingDesktop = false;
        
        // Is the video panel in fullscreen mode
        private bool _fullscreen = false;
        
        // Should display badge on warning or message button
        private bool _haveNewWarning = false;
        private bool _haveNewMessage = false;
        
        // Warning message
        private List<EventItem> _messages = new List<EventItem>();
        private bool _warningVisible = false;

        /// <summary>
        /// Add new warning message
        /// </summary>
        /// <param name="eventItem"></param>
        public void AddWarningMessage(EventItem eventItem)
        {
            _messages.Add(eventItem);
            _haveNewWarning = true;
        }

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(500);
            _showingDesktop = false;
            await OnToggleCamera.InvokeAsync();
        }

        /// <summary>
        /// Add warning message without showing the badge
        /// </summary>
        /// <param name="eventItem"></param>
        public void AddOldMessage(EventItem eventItem)
        {
            _messages.Add(eventItem);
        }

        public void SetCameraLoading(bool loading)
        {
            _cameraLoading = loading;
            StateHasChanged();
        }

        public void SetDesktopLoading(bool loading)
        {
            _desktopLoading = loading;
            StateHasChanged();
        }

        public void SetBanned(bool banned)
        {
            Banned = banned;
            StateHasChanged();
        }


        /// <summary>
        /// Sets the message badge to show
        /// </summary>
        public void SetNewMessage()
        {
            _haveNewMessage = true;
        }
        
        /// <summary>
        /// Callback When the mouse moves in the component
        /// </summary>
        private void OnMouseOver()
        {
            _showToolBar = true;
        }

        /// <summary>
        /// Callback when the mouse moves out of the component
        /// </summary>
        private void OnMouseOut()
        {
            _showToolBar = false;
        }

        /// <summary>
        /// 
        /// </summary>
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

        private void OpenWarning()
        {
            _haveNewWarning = false;
            _warningVisible = true;
        }

        private void CloseWarning()
        {
            _warningVisible = false;
        }

        private async Task OpenMessage()
        {
            _haveNewMessage = false;
            await OnOpenMessage.InvokeAsync();
        }
    }
}