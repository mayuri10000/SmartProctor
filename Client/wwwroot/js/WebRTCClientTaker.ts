// @ts-ignore
window.SmartProctor = window.SmartProctor || {};

namespace SmartProctor {
    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    export class WebRTCClientTaker extends EventTarget {
        private connection: RTCPeerConnection;
        private stream: MediaStream;
        
        // Callbacks that will be used in .NET
        public onIceCandidate: {(this: WebRTCClientTaker, candidate: string): void };
        public onLocalSdp: {(this: WebRTCClientTaker, sdp: string): void };
        public onIceConnectionStateChange: {(this: WebRTCClientTaker, state: string): void };
        public onConnectionStateChange: {(this: WebRTCClientTaker, state: string): void };
        
        public init() {
            this.connection = new RTCPeerConnection();
            this.connection.addTransceiver("video", {direction: 'sendonly'})
            this.connection.onicecandidate = (e) => {
                // Send the ICE candidate info through SignalR in .NET
                this.onIceCandidate(JSON.stringify(e.candidate));
            }
            
            this.connection.oniceconnectionstatechange = (e) => {
                this.onIceConnectionStateChange(this.connection.iceConnectionState);
            }
            
            this.connection.onconnectionstatechange = (e) => {
                this.onConnectionStateChange(this.connection.connectionState);
            }
        }
        
        public async startStreaming() {
            // @ts-ignore
            this.stream = await navigator.mediaDevices.getDisplayMedia();
            var localVideo = document.querySelector(".video.local");
            // @ts-ignore
            localVideo.srcObject = this.stream;
            
            this.stream.getTracks().forEach((track) => {
                this.connection.addTrack(track, this.stream);
            });
            
            var offer = await this.connection.createOffer();
            await this.connection.setLocalDescription(offer);
            
            // Send the local SDP through SignalR in .NET
            this.onLocalSdp(JSON.stringify(offer));
        }
        
        public async receivedAnswerSDP(sdp) {
            await this.connection.setRemoteDescription(JSON.parse(sdp));
        }
        
        public async receivedIceCandidate(candidate) {
            await this.connection.addIceCandidate(JSON.parse(candidate));
        }
        
        public getIceConnectionState() {
            return this.connection.iceConnectionState;
        }
        
        public getConnectionState() {
            return this.connection.connectionState;
        }
    }
    
}

var webRTCClientTaker: SmartProctor.WebRTCClientTaker;

// @ts-ignore
window.SmartProctor.getWebRTCClientTaker = () => {
    if (webRTCClientTaker == null) {
        webRTCClientTaker = new SmartProctor.WebRTCClientTaker();
        webRTCClientTaker.init();
    }
    
    return webRTCClientTaker;
}