using Microsoft.AspNetCore.Components;

namespace SmartProctor.Client.Pages.Exam
{
    public partial class ReshareScreenModal
    {
        private int tipType;
        private string tipText;

        private bool _validScreenShare = false;
        
        [Parameter] public EventCallback OnShareScreen { get; set; }

        [Parameter] public EventCallback OnFinish { get; set; }
        [Parameter] public bool Visible { get; set; }
        
        public bool ShareScreenComplete(string streamLabel)
        {
            if (streamLabel == "screen:0:0" || streamLabel == "Screen 1") // Capable with Chrome and Firefox
            {
                _validScreenShare = true;
                tipType = 1;
                tipText = "Screen capture obtained successfully";
                return true;
            }
            else if (streamLabel.StartsWith("screen"))
            {
                tipType = -1;
                tipText = "Please make sure that you have only one monitor";
            }
            else
            {
                tipType = -1;
                tipText = "Please share your entire screen, instead of a window or browser tab";
            }

            return false;
        }
    }
}