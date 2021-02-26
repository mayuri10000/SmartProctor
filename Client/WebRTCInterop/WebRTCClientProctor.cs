using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.WebRTCInterop
{
    public class WebRTCClientProctor
    {
        private IJSRuntime _jsRuntime;
        private IJSObjectReference _jsObj;

        private DotNetObjectReference<WebRTCClientProctor> _dotRef;
        
        public event EventHandler<(string, RTCIceCandidate)> OnCameraIceCandidate;
        public event EventHandler<(string, RTCIceCandidate)> OnDesktopIceCandidate;
        public event EventHandler<(string, RTCSessionDescriptionInit)> OnCameraSdp;
        public event EventHandler<(string, RTCSessionDescriptionInit)> OnDesktopSdp;
        public event EventHandler<(string, string)> OnDesktopConnectionStateChange;
        public event EventHandler<(string, string)> OnCameraConnectionStateChange;
        
        private WebRTCClientProctor(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        private async ValueTask Init()
        {
            if (_jsObj == null)
            {
                _dotRef = DotNetObjectReference.Create<WebRTCClientProctor>(this);
                var module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/WebRTCClientProctor.js");
                _jsObj = await module.InvokeAsync<IJSObjectReference>("create", _dotRef);
            }
        }

        public async ValueTask OnReceivedDesktopIceCandidate(string testTaker, RTCIceCandidate candidate)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("onReceivedDesktopIceCandidate", testTaker, candidate);
        }

        public async ValueTask OnReceivedDesktopSdp(string testTaker, RTCSessionDescriptionInit sdp)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("onReceivedDesktopSdp", testTaker, sdp);
        }
        
        public async ValueTask OnReceivedCameraIceCandidate(string testTaker, RTCIceCandidate candidate)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("onReceivedCameraIceCandidate", testTaker, candidate);
        }

        public async ValueTask OnReceivedCameraSdp(string testTaker, RTCSessionDescriptionInit sdp)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("onReceivedCameraSdp", testTaker, sdp);
        }

        [JSInvokable]
        private ValueTask _onDesktopConnectionStateChange(string testTaker, string state)
        {
            OnDesktopConnectionStateChange?.Invoke(this, (testTaker, state));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        private ValueTask _onDesktopIceCandidate(string testTaker, RTCIceCandidate candidate)
        {
            OnDesktopIceCandidate?.Invoke(this, (testTaker, candidate));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        private ValueTask _onDesktopSdp(string testTaker, RTCSessionDescriptionInit sdp)
        {
            OnDesktopSdp?.Invoke(this, (testTaker, sdp));
            return ValueTask.CompletedTask;
        }
        
        [JSInvokable]
        private ValueTask _onCameraConnectionStateChange(string testTaker, string state)
        {
            OnCameraConnectionStateChange?.Invoke(this, (testTaker, state));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        private ValueTask _onCameraIceCandidate(string testTaker, RTCIceCandidate candidate)
        {
            OnCameraIceCandidate?.Invoke(this, (testTaker, candidate));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        private ValueTask _onCameraSdp(string testTaker, RTCSessionDescriptionInit sdp)
        {
            OnCameraSdp?.Invoke(this, (testTaker, sdp));
            return ValueTask.CompletedTask;
        }
    }
}