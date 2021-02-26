namespace SmartProctor {
    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    export class WebRTCClientTaker {
        private proctorConnections: { [userName: string]: RTCPeerConnection } = {};
        private cameraConnection: RTCPeerConnection;
        private desktopStream: MediaStream;
        public helper;

        // Callbacks that will be used in .NET
        public onIceCandidate: { (this: WebRTCClientTaker, candidate: string): void };
        public onLocalSdp: { (this: WebRTCClientTaker, sdp: string): void };
        public onIceConnectionStateChange: { (this: WebRTCClientTaker, state: string): void };
        public onConnectionStateChange: { (this: WebRTCClientTaker, state: string): void };

        public async init(helper, proctors: string[]) {
            this.helper = helper;
            this.cameraConnection = new RTCPeerConnection();

            this.cameraConnection.onicecandidate = async (e) => {
                await helper.invokeMethodAsync("_onCameraIceCandidate", e.candidate);
            };

            this.cameraConnection.onconnectionstatechange = async (e) => {
                await helper.invokeMethodAsync("_onCameraConnectionStateChange", this.cameraConnection.connectionState);
            };
            
            this.cameraConnection.ontrack = async (e) => {
                // @ts-ignore
                document.getElementById("local-desktop").srcObject = e.streams[0];
                this.desktopStream = e.streams[0];
            }

            for (let proctor in proctors) {
                let conn = new RTCPeerConnection();

                conn.onicecandidate = async (e) => {
                    await helper.invokeMethodAsync("_onProctorIceCandidate", proctor, e.candidate);
                };

                conn.onconnectionstatechange = async (e) => {
                    await helper.invokeMethodAsync("_onProctorConnectionStateChange", proctor, conn.connectionState);
                };
                this.proctorConnections[proctor] = conn;
            }
        }

        public async startStreaming() {
            // @ts-ignore
            this.desktopStream = await navigator.mediaDevices.getDisplayMedia();
            let localVideo = document.querySelector(".video.local");
            // @ts-ignore
            localVideo.srcObject = this.desktopStream;
            

            for (let proctor in this.proctorConnections) {
                let conn = this.proctorConnections[proctor];
                this.desktopStream.getTracks().forEach((track) => {
                    conn.addTrack(track, this.desktopStream);
                });
                
                let offer = await conn.createOffer();
                await conn.setLocalDescription(offer);

                // Send the local SDP through SignalR in .NET
                await this.helper.invokeMethodAsync("_onProctorSdp", proctor, offer);
            }
        }

        public async receivedProctorAnswerSDP(proctor: string, sdp: RTCSessionDescriptionInit) {
            await this.proctorConnections[proctor].setRemoteDescription(sdp);
        }

        public async receivedProctorIceCandidate(proctor: string, candidate: RTCIceCandidate) {
            await this.proctorConnections[proctor].addIceCandidate(candidate);
        }

        public async receivedCameraAnswerSDP(sdp: RTCSessionDescriptionInit) {
            await this.cameraConnection.setRemoteDescription(sdp);
        }

        public async receivedCameraIceCandidate(candidate: RTCIceCandidate) {
            await this.cameraConnection.addIceCandidate(candidate);
        }
    }

}

var webRTCClientTaker: SmartProctor.WebRTCClientTaker;

export function create(helper, proctors: string[]) {
    if (webRTCClientTaker == null) {
        webRTCClientTaker = new SmartProctor.WebRTCClientTaker();
        webRTCClientTaker.init(helper, proctors);
    }

    return webRTCClientTaker;
}