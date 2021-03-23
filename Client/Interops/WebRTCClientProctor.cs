using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.Interops
{
    public class WebRTCClientProctor
    {
        private IJSRuntime _jsRuntime;
        private IJSObjectReference _jsObj;

        private DotNetObjectReference<WebRTCClientProctor> _dotRef;
        private string[] _testTakers;

        public event EventHandler<(string, RTCIceCandidate)> OnCameraIceCandidate;
        public event EventHandler<(string, RTCIceCandidate)> OnDesktopIceCandidate;
        public event EventHandler<(string, RTCSessionDescriptionInit)> OnCameraSdp;
        public event EventHandler<(string, RTCSessionDescriptionInit)> OnDesktopSdp;
        public event EventHandler<(string, string)> OnDesktopConnectionStateChange;
        public event EventHandler<(string, string)> OnCameraConnectionStateChange;
        public event EventHandler<string> OnCameraMuted;
        public event EventHandler<string> OnCameraUnmuted;
        public event EventHandler<string> OnDesktopMuted;
        public event EventHandler<string> OnDesktopUnmuted;


        public WebRTCClientProctor(IJSRuntime jsRuntime, string[] testTakers)
        {
            _jsRuntime = jsRuntime;
            _testTakers = testTakers;
        }

        private async ValueTask Init()
        {
            if (_jsObj == null)
            {
                _dotRef = DotNetObjectReference.Create<WebRTCClientProctor>(this);
                var module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/WebRTCClientProctor.js");
                _jsObj = await module.InvokeAsync<IJSObjectReference>("create", _dotRef, _testTakers);
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

        public async ValueTask SetDesktopVideoElem(string testTaker, string elementId)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("setDesktopVideoElem", testTaker, elementId);
        }

        public async ValueTask SetCameraVideoElem(string testTaker, string elementId)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("setCameraVideoElem", testTaker, elementId);
        }

        [JSInvokable]
        public ValueTask _onDesktopConnectionStateChange(string testTaker, string state)
        {
            OnDesktopConnectionStateChange?.Invoke(this, (testTaker, state));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onDesktopIceCandidate(string testTaker, RTCIceCandidate candidate)
        {
            OnDesktopIceCandidate?.Invoke(this, (testTaker, candidate));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onDesktopSdp(string testTaker, RTCSessionDescriptionInit sdp)
        {
            OnDesktopSdp?.Invoke(this, (testTaker, sdp));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraConnectionStateChange(string testTaker, string state)
        {
            OnCameraConnectionStateChange?.Invoke(this, (testTaker, state));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraIceCandidate(string testTaker, RTCIceCandidate candidate)
        {
            OnCameraIceCandidate?.Invoke(this, (testTaker, candidate));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraSdp(string testTaker, RTCSessionDescriptionInit sdp)
        {
            OnCameraSdp?.Invoke(this, (testTaker, sdp));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraMuted(string testTaker)
        {
            OnCameraMuted?.Invoke(this, testTaker);
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraUnmuted(string testTaker)
        {
            OnCameraUnmuted?.Invoke(this, testTaker);
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onDesktopMuted(string testTaker)
        {
            OnDesktopMuted?.Invoke(this, testTaker);
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onDesktopUnmuted(string testTaker)
        {
            OnDesktopUnmuted?.Invoke(this, testTaker);
            return ValueTask.CompletedTask;
        }
    }
}