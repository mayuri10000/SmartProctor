namespace SmartProctor {
    class TestTakerConnection {
        public desktopConnection: RTCPeerConnection;
        public cameraConnection: RTCPeerConnection;
        
        public desktopStream: MediaStream;
        public cameraStream: MediaStream;
        
        public desktopVideoElem: Element;
        public cameraVideoElem: Element;
    }
    /**
     * Typescript implementation of the WebRTC related functions in the proctor side
     */
    export class WebRTCClientProctor {
        public helper;
        private testTakerConnections: { [userName: string]: TestTakerConnection } = {};
        
        public async init(helper, testTakers: string[]) {
            this.helper = helper;
            testTakers.forEach((testTaker) => {
                var desktopConnection = new RTCPeerConnection(null);
                var cameraConnection = new RTCPeerConnection(null);
                
                this.testTakerConnections[testTaker] = {
                    cameraVideoElem: document.getElementById(testTaker + "-video"),
                    desktopVideoElem: null,
                    desktopConnection: desktopConnection,
                    cameraConnection: cameraConnection,
                    desktopStream: null,
                    cameraStream: null
                };

                // @ts-ignore
                desktopConnection.onaddstream = (e) => {
                    if (this.testTakerConnections[testTaker].desktopVideoElem != null)
                        // @ts-ignore
                        this.testTakerConnections[testTaker].desktopVideoElem.srcObject = e.stream;
                    this.testTakerConnections[testTaker].desktopStream = e.stream;
                    console.log("get desktop stream: " + testTaker);
                };
                
                desktopConnection.onconnectionstatechange = async (e) => {
                    console.log("connection state of " + testTaker + " changed to '" + desktopConnection.connectionState + "'");
                    await this.helper.invokeMethodAsync("_onDesktopConnectionStateChange", testTaker, desktopConnection.connectionState);
                };
                
                desktopConnection.onicecandidate = async (e) => {
                    await this.helper.invokeMethodAsync("_onDesktopIceCandidate", testTaker, e.candidate);
                }

                // @ts-ignore
                cameraConnection.onaddstream = (e) => {
                    if (this.testTakerConnections[testTaker].cameraVideoElem != null)
                        // @ts-ignore
                        this.testTakerConnections[testTaker].cameraVideoElem.srcObject = e.stream;
                    this.testTakerConnections[testTaker].cameraStream = e.stream;
                };

                cameraConnection.onconnectionstatechange = async (e) => {
                    await this.helper.invokeMethodAsync("_onCameraConnectionStateChange", testTaker, desktopConnection.connectionState);
                };

                cameraConnection.onicecandidate = async (e) => {
                    await this.helper.invokeMethodAsync("_onCameraIceCandidate", testTaker, e.candidate);
                }
            });
        }
        
        public async onReceivedDesktopIceCandidate(testTaker: string, candidate: RTCIceCandidate) {
            await this.testTakerConnections[testTaker].desktopConnection.addIceCandidate(candidate);
            console.log("received ICE candidate from " + testTaker + ".");
        }
        
        public async onReceivedDesktopSdp(testTaker: string, sdp: RTCSessionDescriptionInit) {
            let conn = this.testTakerConnections[testTaker].desktopConnection;
            await conn.setRemoteDescription(sdp);
            let answer = await conn.createAnswer();
            await conn.setLocalDescription(answer);
            await this.helper.invokeMethodAsync("_onDesktopSdp", testTaker, answer);
            console.log("received offer from " + testTaker + " and sending answer.");
        }

        public async onReceivedCameraIceCandidate(testTaker: string, candidate: RTCIceCandidate) {
            await this.testTakerConnections[testTaker].cameraConnection.addIceCandidate(candidate);
        }

        public async onReceivedCameraSdp(testTaker: string, sdp: RTCSessionDescriptionInit) {
            let conn = this.testTakerConnections[testTaker].cameraConnection;
            await conn.setRemoteDescription(sdp);
            let answer = await conn.createAnswer();
            await conn.setLocalDescription(answer);
            await this.helper.invokeMethodAsync("_onCameraSdp", testTaker, answer);
        }
        
        public async setDesktopVideoElem(testTaker: string, elementId: string) {
            if (this.testTakerConnections[testTaker].desktopVideoElem != null)
                // @ts-ignore
                this.testTakerConnections[testTaker].desktopVideoElem.srcObject = null;
            this.testTakerConnections[testTaker].desktopVideoElem = document.getElementById(elementId);
            // @ts-ignore
            this.testTakerConnections[testTaker].desktopVideoElem.srcObject = this.testTakerConnections[testTaker].desktopStream;
        }

        public async setCameraVideoElem(testTaker: string, elementId: string) {
            if (this.testTakerConnections[testTaker].cameraVideoElem != null)
                // @ts-ignore
                this.testTakerConnections[testTaker].cameraVideoElem.srcObject = null;
            this.testTakerConnections[testTaker].cameraVideoElem = document.getElementById(elementId);
            // @ts-ignore
            this.testTakerConnections[testTaker].cameraVideoElem.srcObject = this.testTakerConnections[testTaker].cameraStream;
        }
    }
}

let webRTCClientProctor: SmartProctor.WebRTCClientProctor;

export function create(helper, testTakers: string[]) {
    if (webRTCClientProctor == null) {
        webRTCClientProctor = new SmartProctor.WebRTCClientProctor();
        webRTCClientProctor.init(helper, testTakers);
    }

    return webRTCClientProctor;
}