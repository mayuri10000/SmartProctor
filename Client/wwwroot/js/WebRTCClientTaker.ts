namespace SmartProctor {
    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    export class WebRTCClientTaker {
        private proctorConnections: { [userName: string]: RTCPeerConnection } = {};
        private cameraConnection: RTCPeerConnection;
        private desktopStream: MediaStream;
        private cameraStream: MediaStream;
        public helper;

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
                this.cameraStream = e.streams[0];
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

        public async startStreamingDesktop() {
            // @ts-ignore
            this.desktopStream = await navigator.mediaDevices.getDisplayMedia();

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
        
        public setDesktopVideoElement(elementId: string) {
            let localVideo = document.getElementById(elementId);
            // @ts-ignore
            localVideo.srcObject = this.desktopStream;
        }
        
        public setCameraVideoElement(elementId: string) {
            let localVideo = document.getElementById(elementId);
            // @ts-ignore
            localVideo.srcObject = this.cameraStream;
        }

        public async receivedProctorAnswerSDP(proctor: string, sdp: RTCSessionDescriptionInit) {
            await this.proctorConnections[proctor].setRemoteDescription(sdp);
        }

        public async receivedProctorIceCandidate(proctor: string, candidate: RTCIceCandidate) {
            await this.proctorConnections[proctor].addIceCandidate(candidate);
        }

        public async receivedCameraOfferSDP(sdp: RTCSessionDescriptionInit) {
            await this.cameraConnection.setRemoteDescription(sdp);
            let answer = await this.cameraConnection.createAnswer();
            await this.cameraConnection.setLocalDescription(answer);
            await this.helper.invokeMethodAsync("_onCameraSdp", answer);
        }

        public async receivedCameraIceCandidate(candidate: RTCIceCandidate) {
            await this.cameraConnection.addIceCandidate(candidate);
        }
    }

}

let webRTCClientTaker: SmartProctor.WebRTCClientTaker;

export function create(helper, proctors: string[]) {
    if (webRTCClientTaker == null) {
        webRTCClientTaker = new SmartProctor.WebRTCClientTaker();
        webRTCClientTaker.init(helper, proctors);
    }

    return webRTCClientTaker;
}