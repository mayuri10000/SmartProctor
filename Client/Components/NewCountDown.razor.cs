using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;

namespace SmartProctor.Client.Components
{
    /// <summary>
    /// Ant Design's CountDown component will not continue to count down when the page is hidden
    /// (like when the browser tab switched). Also, we need to implement a CountDown component that
    /// can alert the test taker when the exam will soon be ended.
    /// </summary>
    public partial class NewCountDown
    {
        /// <summary>
        /// Time to count to 
        /// </summary>
        [Parameter]
        public DateTime Deadline { get; set; }
        
        /// <summary>
        /// Seconds before the deadline when the component should show a alert,
        /// null if no alert needed.
        /// </summary>
        [Parameter]
        public int? SecondsBeforeAlert { get; set; }
        
        /// <summary>
        /// Callback when the time's up
        /// </summary>
        [Parameter]
        public EventCallback OnEnded { get; set; }
        
        private TimeSpan _currentTime;
        private string _displayString;
        private Timer _timer;
        private bool _isAlert = false;
        private bool _isEnded = false;
        
        protected override void OnParametersSet()
        {
            _currentTime = TimeSpan.FromMinutes(1);
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimer;
            _timer.Enabled = true;
        }

        /// <summary>
        /// Timer callback
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimer(object source, ElapsedEventArgs e)
        {
            if (_isEnded) 
                return;
            
            if (_currentTime > TimeSpan.Zero)
            {
                _currentTime = Deadline - DateTime.Now;
                _displayString = _currentTime.ToString();
                // Remove the part after second of the remaining time.
                _displayString = _displayString.Substring(0, _displayString.LastIndexOf("."));
                InvokeAsync(StateHasChanged);
            }
            else
            {
                _timer.Enabled = false;
                _isEnded = true;
                InvokeAsync(OnEnded.InvokeAsync);
            }

            if (!_isAlert && SecondsBeforeAlert != null &&
                _currentTime.Subtract(TimeSpan.FromSeconds(SecondsBeforeAlert.Value)) <= TimeSpan.Zero)
            {
                _isAlert = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _timer.Dispose();
        }
    }
}