using System;
using System.Threading.Tasks;
using BrowserInterop.Extensions;
using Microsoft.JSInterop;
using SmartProctor.Shared.WebRTC;

namespace SmartProctor.Client.WebRTCInterop
{
    public class WebRTCClientTaker
    {
        private IJSRuntime _jsRuntime;
        private IJSObjectReference _jsObj;

        private DotNetObjectReference<WebRTCClientTaker> _dotRef;
        private string[] _proctors;
        
        public event EventHandler<RTCIceCandidate> OnCameraIceCandidate;
        public event EventHandler<(string, RTCIceCandidate)> OnProctorIceCandidate;
        public event EventHandler<RTCSessionDescriptionInit> OnCameraSdp;
        public event EventHandler<(string, RTCSessionDescriptionInit)> OnProctorSdp;
        public event EventHandler<string> OnCameraConnectionStateChange;
        public event EventHandler<(string, string)> OnProctorConnectionStateChange;

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
                _jsObj = await module.InvokeAsync<IJSObjectReference>("create", _dotRef, _proctors);
            }
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
        
        public async ValueTask StartStreamingDesktop()
        {
            await Init();
            await _jsObj.InvokeVoidAsync("startStreamingDesktop");
        }

        public async ValueTask ReceivedCameraAnswerSDP(RTCSessionDescriptionInit sdp)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedAnswerSDP", sdp);
        }

        public async ValueTask ReceivedCameraIceCandidate(RTCIceCandidate candidate)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedIceCandidate", candidate);
        }
        
        public async ValueTask ReceivedProctorAnswerSDP(string proctor, RTCSessionDescriptionInit sdp)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedAnswerSDP", proctor, sdp);
        }

        public async ValueTask ReceivedProctorIceCandidate(string proctor, RTCIceCandidate candidate)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedIceCandidate", proctor, candidate);
        }


        [JSInvokable]
        public ValueTask _onCameraIceCandidate(RTCIceCandidate candidate)
        {
            OnCameraIceCandidate?.Invoke(this, candidate);
            return ValueTask.CompletedTask;
        }
        
        [JSInvokable]
        public ValueTask _onProctorIceCandidate(string proctor, RTCIceCandidate candidate)
        {
            OnProctorIceCandidate?.Invoke(this, (proctor, candidate));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraSdp(RTCSessionDescriptionInit sdp)
        {
            OnCameraSdp?.Invoke(this, sdp);
            return ValueTask.CompletedTask;
        }
        
        [JSInvokable]
        public ValueTask _onProctorSdp(string proctor, RTCSessionDescriptionInit sdp)
        {
            OnProctorSdp?.Invoke(this, (proctor, sdp));
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onCameraConnectionStateChange(string connectionState)
        {
            OnCameraConnectionStateChange?.Invoke(this, connectionState);
            return ValueTask.CompletedTask;
        }
        
        [JSInvokable]
        public ValueTask _onProctorConnectionStateChange(string proctor, string connectionState)
        {
            OnProctorConnectionStateChange?.Invoke(this, (proctor, connectionState));
            return ValueTask.CompletedTask;
        }
    }
}