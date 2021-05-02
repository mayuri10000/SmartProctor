using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Components
{
    /// <summary>
    /// Component used for chat between proctor and test takers, contains a message list and a input box.
    /// </summary>
    public partial class ChatDrawer
    {
        /// <summary>
        /// Is this component displayed on the proctoring page, if so, the handler should not
        /// be displayed since the proctor should chat with many test takers in different conversation.
        /// </summary>
        [Parameter]
        public bool ForProctoring { get; set; }
        
        /// <summary>
        /// The user ID to chat with, null for broadcast messages. Should always be null when used for test
        /// taker's page since test takers should not send individual message to a specific proctor.
        /// </summary>
        [Parameter]
        public string ChatWith { get; set; }
        
        /// <summary>
        /// The list of message to display
        /// </summary>
        [Parameter]
        public IList<EventItem> Messages { get; set; }

        /// <summary>
        /// Whether the drawer is visible
        /// </summary>
        [Parameter]
        public bool Visible { get; set; }
        
        /// <summary>
        /// Callback when a message is about to send
        /// </summary>
        [Parameter]
        public EventCallback<string> OnSendMessage { get; set; }

        private string _message = "";     // Current message, binds to the input box
        private int _newMessageCount = 0; // New message count, displayed on the badge of the handle

        /// <summary>
        /// Called when the handle is clicked
        /// </summary>
        private void SetVisible()
        {
           Visible = !Visible;
           if (Visible)
           {
               _newMessageCount = 0;
           }
        }

        /// <summary>
        /// Invokes the message callback and remove the text in the input box
        /// </summary>
        private async Task SendMessage()
        {
            await OnSendMessage.InvokeAsync(_message);
            _message = "";
        }

        /// <summary>
        /// Increment the new message count on the badge
        /// </summary>
        public void IncrementMessage()
        {
            _newMessageCount++;
            StateHasChanged();
        }
    }
}