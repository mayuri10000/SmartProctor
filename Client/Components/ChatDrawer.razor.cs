using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Components
{
    public partial class ChatDrawer
    {
        
        [Parameter]
        public bool ForProctoring { get; set; }
        
        [Parameter]
        public string ChatWith { get; set; }
        
        [Parameter]
        public IList<EventItem> Messages { get; set; }

        [Parameter]
        public bool Visible { get; set; }
        
        [Parameter]
        public EventCallback<string> OnSendMessage { get; set; }

        private string _message = "";
        private int _newMessageCount = 0;

        private void SetVisible()
        {
           Visible = !Visible;
           if (Visible)
           {
               _newMessageCount = 0;
           }
        }

        private async Task SendMessage()
        {
            await OnSendMessage.InvokeAsync(_message);
            _message = "";
        }

        public void IncrementMessage()
        {
            _newMessageCount++;
            StateHasChanged();
        }
    }
}