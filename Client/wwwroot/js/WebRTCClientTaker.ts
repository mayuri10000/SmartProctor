namespace SmartProctor {
    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    export class WebRTCClientTaker {
        private proctorConnections: { [userName: string]: RTCPeerConnection } = {};
        private cameraConnection: RTCPeerConnection;
        private desktopStream: MediaStream;
        private cameraStream: MediaStream;
        private desktopVideoElem: Element;
        private cameraVideoElem: Element;
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

            proctors.forEach((proctor) => {
                let conn = new RTCPeerConnection(null);

                conn.onicecandidate = async (e) => {
                    console.log("Sending ICE candidate to " + proctor + ".");
                    await helper.invokeMethodAsync("_onProctorIceCandidate", proctor, e.candidate);
                };

                conn.onconnectionstatechange = async (e) => {
                    console.log("connection state of " + proctors + " changed to '" + conn.connectionState + "'");
                    await helper.invokeMethodAsync("_onProctorConnectionStateChange", proctor, conn.connectionState);
                };
                this.proctorConnections[proctor] = conn;
            });
        }
        
        public async obtainDesktopStream() : Promise<string> {
            // @ts-ignore
            this.desktopStream = await navigator.mediaDevices.getDisplayMedia();
            this.desktopStream.getTracks()[0].onmute = async (_) => {
                await this.helper.invokeMethodAsync("_onDesktopMuted");
            };
            return this.desktopStream.getTracks()[0].label;
        }

        public async startStreamingDesktop() {
            for (let proctor in this.proctorConnections) {
                let conn = this.proctorConnections[proctor];
                this.desktopStream.getTracks().forEach((track) => {
                    conn.addTrack(track, this.desktopStream);
                });
                
                let offer = await conn.createOffer();
                await conn.setLocalDescription(offer);

                // Send the local SDP through SignalR in .NET
                await this.helper.invokeMethodAsync("_onProctorSdp", proctor, offer);
                console.log("Sending offer to " + proctor + ".");
            }
        }
        
        public async reconnectToProctor(proctor: string) {
            let conn = this.proctorConnections[proctor];

            let offer = await conn.createOffer();
            await conn.setLocalDescription(offer);

            // Send the local SDP through SignalR in .NET
            await this.helper.invokeMethodAsync("_onProctorSdp", proctor, offer);
            console.log("Sending offer to " + proctor + ".");
        }
        
        public setDesktopVideoElement(elementId: string) {
            if (this.desktopVideoElem != null)
                // @ts-ignore
                this.desktopVideoElem.srcObject = null;
            this.desktopVideoElem = document.getElementById(elementId);
            // @ts-ignore
            this.desktopVideoElem.srcObject = this.desktopStream;
        }
        
        public setCameraVideoElement(elementId: string) {
            if (this.cameraVideoElem != null)
                // @ts-ignore
                this.cameraVideoElem.srcObject = null;
            this.cameraVideoElem = document.getElementById(elementId);
            // @ts-ignore
            this.cameraVideoElem.srcObject = this.cameraStream;
        }

        public async receivedProctorAnswerSDP(proctor: string, sdp: RTCSessionDescriptionInit) {
            await this.proctorConnections[proctor].setRemoteDescription(sdp);
            console.log("received answer from " + proctor + " and sending answer.");
        }

        public async receivedProctorIceCandidate(proctor: string, candidate: RTCIceCandidate) {
            await this.proctorConnections[proctor].addIceCandidate(candidate);
            console.log("received ICE candidate from " + proctor + ".");
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
        
        public async onProctorReconnected(proctor: string) {
            console.log("Proctor " + proctor + " reconnected, resending SDP...");
            let conn = this.proctorConnections[proctor];
            this.desktopStream.getTracks().forEach((track) => {
                conn.addTrack(track, this.desktopStream);
            });

            let offer = await conn.createOffer();
            await conn.setLocalDescription(offer);

            // Send the local SDP through SignalR in .NET
            await this.helper.invokeMethodAsync("_onProctorSdp", proctor, offer);
            console.log("Sending offer to " + proctor + ".");
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