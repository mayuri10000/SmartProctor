using System;
using System.Threading.Tasks;
using BrowserInterop.Extensions;
using Microsoft.JSInterop;

namespace SmartProctor.Client.WebRTCInterop
{
    public class WebRTCClientTaker
    {
        private IJSRuntime _jsRuntime;
        private JsRuntimeObjectRef _jsObj;
        
        public event EventHandler<string> OnIceCandidate;
        public event EventHandler<string> OnLocalSdp;
        public event EventHandler<string> OnIceConnectionStateChange;
        public event EventHandler<string> OnConnectionStateChange;

        private WebRTCClientTaker(IJSRuntime jsRuntime, JsRuntimeObjectRef jsObj)
        {
            _jsObj = jsObj;
            _jsRuntime = jsRuntime;
        }

        public async ValueTask<WebRTCClientTaker> GetInstance(IJSRuntime jsRuntime)
        {
            var window = await jsRuntime.GetWindowPropertyRef("SmartProctor")
                .ConfigureAwait(false);
            var obj = await jsRuntime.InvokeInstanceMethodGetRef(window, "getWebRTCClientTaker")
                .ConfigureAwait(false);
            
            var ret = new WebRTCClientTaker(jsRuntime, obj);
            await ret.ConfigureCallbacks();
            
            return ret;
        }

        public async ValueTask StartStreaming()
        {
            await _jsRuntime.InvokeInstanceMethod(_jsObj, "startStreaming").ConfigureAwait(false);
        }

        public async ValueTask ReceivedAnswerSDP(string sdp)
        {
            await _jsRuntime.InvokeInstanceMethod(_jsObj, "receivedAnswerSDP", sdp).ConfigureAwait(false);
        }

        public async ValueTask ReceivedIceCandidate(string candidate)
        {
            await _jsRuntime.InvokeInstanceMethod(_jsObj, "receivedIceCandidate", candidate).ConfigureAwait(false);
        }

        public async ValueTask<string> GetIceConnectionState()
        {
            return await _jsRuntime.InvokeInstanceMethod<string>(_jsObj, "getIceConnectionState").ConfigureAwait(false);
        }

        public async ValueTask<string> GetConnectionState()
        {
            return await _jsRuntime.InvokeInstanceMethod<string>(_jsObj, "getConnectionState").ConfigureAwait(false);
        }

        private async ValueTask ConfigureCallbacks()
        {
            await _jsRuntime.AddEventListener(_jsObj, "self", "onIceCandidate", CallBackInteropWrapper.Create(
                async (string candidate) =>
                {
                    OnIceCandidate?.Invoke(this, candidate);
                })).ConfigureAwait(false);
            await _jsRuntime.AddEventListener(_jsObj, "self", "onLocalSdp", CallBackInteropWrapper.Create(
                async (string candidate) =>
                {
                    OnLocalSdp?.Invoke(this, candidate);
                })).ConfigureAwait(false);
            await _jsRuntime.AddEventListener(_jsObj, "self", "onIceConnectionStateChange", CallBackInteropWrapper.Create(
                async (string candidate) =>
                {
                    OnIceConnectionStateChange?.Invoke(this, candidate);
                })).ConfigureAwait(false);
            await _jsRuntime.AddEventListener(_jsObj, "self", "onConnectionStateChange", CallBackInteropWrapper.Create(
                async (string candidate) =>
                {
                    OnConnectionStateChange?.Invoke(this, candidate);
                })).ConfigureAwait(false);
        }
    }
}