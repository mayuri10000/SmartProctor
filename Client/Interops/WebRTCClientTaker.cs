using System;
using System.Threading.Tasks;
using BrowserInterop.Extensions;
using Microsoft.JSInterop;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Interops
{
    public class WebRTCClientTaker
    {
        private IJSRuntime _jsRuntime;
        private IJSObjectReference _jsObj;

        private DotNetObjectReference<WebRTCClientTaker> _dotRef;
        private string[] _proctors;
        
        public event EventHandler<(string, RTCIceCandidate)> OnCameraIceCandidate;
        public event EventHandler<(string, RTCSessionDescriptionInit)> OnCameraSdp;
        public event EventHandler<(string, RTCIceCandidate)> OnDesktopIceCandidate;
        public event EventHandler<(string, RTCSessionDescriptionInit)> OnDesktopSdp;
        public event EventHandler<(string, string)> OnCameraConnectionStateChange;
        public event EventHandler<(string, string)> OnDesktopConnectionStateChange;
        public event EventHandler OnDesktopInactivated;
        public event EventHandler OnCameraInactivated;

        public WebRTCClientTaker(IJSRuntime jsRuntime, string[] proctors)
        {
            _jsRuntime = jsRuntime;
            _proctors = proctors;
        }

        private async ValueTask Init()
        {
            if (_jsObj == null)
            {
                _dotRef = DotNetObjectReference.Create<WebRTCClientTaker>(this);
                var module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/WebRTCClientTaker.js");
                _jsObj = await module.InvokeAsync<IJSObjectReference>("create", _dotRef, null, _proctors);
            }
        }

        public async ValueTask<string> ObtainDesktopStream()
        {
            await Init();
            return await _jsObj.InvokeAsync<string>("obtainDesktopStream");
        }

        public async ValueTask ObtainCameraStream(string mjpegUrl)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("obtainCameraStream", mjpegUrl);
        }

        public async ValueTask StartStreaming()
        {
            await Init();
            await _jsObj.InvokeVoidAsync("startStreaming");
        }

        public async ValueTask ReconnectToProctor(string proctor)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("reconnectToProctor");
        }

        public async ValueTask SetDesktopVideoElement(string elementId)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("setDesktopVideoElement", elementId);
        }

        public async ValueTask SetCameraVideoElement(string elementId)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("setCameraVideoElement", elementId);
        }

        public async ValueTask ReceivedDesktopAnswerSDP(string proctor, RTCSessionDescriptionInit sdp)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedDesktopAnswerSDP", proctor, sdp);
        }

        public async ValueTask ReceivedDesktopIceCandidate(string proctor, RTCIceCandidate candidate)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedDesktopIceCandidate", proctor, candidate);
        }
        
        public async ValueTask ReceivedCameraAnswerSDP(string proctor, RTCSessionDescriptionInit sdp)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedCameraAnswerSDP", proctor, sdp);
        }

        public async ValueTask ReceivedCameraIceCandidate(string proctor, RTCIceCandidate candidate)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedCameraIceCandidate", proctor, candidate);
        }
        
        [JSInvokable]
        public ValueTask _onCameraIceCandidate(string proctor, RTCIceCandidate candidate)
        {
            OnCameraIceCandidate?.Invoke(this, (proctor, candidate));
            return ValueTask.CompletedTask;
        }
        
        [JSInvokable]
        public ValueTask _onDesktopIceCandidate(string proctor, RTCIceCandidate candidate)
        {
            OnDesktopIceCandidate?.Invoke(this, (proctor, candidate));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraSdp(string proctor, RTCSessionDescriptionInit sdp)
        {
            OnCameraSdp?.Invoke(this, (proctor, sdp));
            return ValueTask.CompletedTask;
        }
        
        [JSInvokable]
        public ValueTask _onDesktopSdp(string proctor, RTCSessionDescriptionInit sdp)
        {
            OnDesktopSdp?.Invoke(this, (proctor, sdp));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraConnectionStateChange(string proctor, string connectionState)
        {
            OnCameraConnectionStateChange?.Invoke(this, (proctor, connectionState));
            return ValueTask.CompletedTask;
        }
        
        [JSInvokable]
        public ValueTask _onDesktopConnectionStateChange(string proctor, string connectionState)
        {
            OnDesktopConnectionStateChange?.Invoke(this, (proctor, connectionState));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onDesktopInactivated()
        {
            OnDesktopInactivated?.Invoke(this, EventArgs.Empty);
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraInactivated()
        {
            OnCameraInactivated?.Invoke(this, EventArgs.Empty);
            return ValueTask.CompletedTask;
        }
    }
}