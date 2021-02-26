using System;
using System.Threading.Tasks;
using BrowserInterop.Extensions;
using Microsoft.JSInterop;

namespace SmartProctor.Client.WebRTCInterop
{
    public class WebRTCClientTaker
    {
        private IJSRuntime _jsRuntime;
        private IJSObjectReference _jsObj;

        private DotNetObjectReference<WebRTCClientTaker> _dotRef;
        
        public event EventHandler<RTCIceCandidate> OnIceCandidate;
        public event EventHandler<RTCSessionDescriptionInit> OnLocalSdp;
        public event EventHandler<string> OnIceConnectionStateChange;
        public event EventHandler<string> OnConnectionStateChange;

        private WebRTCClientTaker(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        private async ValueTask Init()
        {
            if (_jsObj == null)
            {
                _dotRef = DotNetObjectReference.Create<WebRTCClientTaker>(this);
                var module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/WebRTCClientTaker.js");
                _jsObj = await module.InvokeAsync<IJSObjectReference>("create", _dotRef);
            }
        }

        public async ValueTask StartStreaming()
        {
            await Init();
            await _jsObj.InvokeVoidAsync("startStreaming");
        }

        public async ValueTask ReceivedAnswerSDP(RTCSessionDescriptionInit sdp)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedAnswerSDP", sdp);
        }

        public async ValueTask ReceivedIceCandidate(RTCIceCandidate candidate)
        {
            await Init();
            await _jsObj.InvokeVoidAsync("receivedIceCandidate", candidate);
        }

        public async ValueTask<string> GetIceConnectionState()
        {
            await Init();
            return await _jsObj.InvokeAsync<string>("getIceConnectionState");
        }

        public async ValueTask<string> GetConnectionState()
        {
            await Init();
            return await _jsObj.InvokeAsync<string>("getConnectionState");
        }

        [JSInvokable]
        public ValueTask _onIceCandidate(RTCIceCandidate candidate)
        {
            OnIceCandidate?.Invoke(this, candidate);
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onIceConnectionStateChange(string iceConnectionState)
        {
            OnIceConnectionStateChange?.Invoke(this, iceConnectionState);
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onLocalSdp(RTCSessionDescriptionInit sdp)
        {
            OnLocalSdp?.Invoke(this, sdp);
            return ValueTask.CompletedTask;
        }

        [JSInvokable]
        public ValueTask _onConnectionStateChange(string connectionState)
        {
            OnConnectionStateChange?.Invoke(this, connectionState);
            return ValueTask.CompletedTask;
        }
    }
}