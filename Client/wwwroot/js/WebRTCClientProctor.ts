namespace SmartProctor {
    class TestTakerConnection {
        public desktopConnection: RTCPeerConnection;
        public cameraConnection: RTCPeerConnection;
        
        public desktopStream: MediaStream;
        public cameraStream: MediaStream;
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
                var desktopConnection = new RTCPeerConnection();
                var cameraConnection = new RTCPeerConnection();
                
                this.testTakerConnections[testTaker] = {
                    desktopConnection: desktopConnection,
                    cameraConnection: cameraConnection,
                    desktopStream: null,
                    cameraStream: null
                };
                
                desktopConnection.ontrack = (e) => {
                    // @ts-ignore
                    document.getElementById(testTaker + "-desktop").srcObject = e.streams[0];
                    this.testTakerConnections[testTaker].desktopStream = e.streams[0];
                };
                
                desktopConnection.onconnectionstatechange = async (e) => {
                    await this.helper.invokeMethodAsync("_onDesktopConnectionStateChange", testTaker, desktopConnection.connectionState);
                };
                
                desktopConnection.onicecandidate = async (e) => {
                    await this.helper.invokeMethodAsync("_onDesktopIceCandidate", testTaker, desktopConnection.connectionState);
                }
                
                cameraConnection.ontrack = (e) => {
                    // @ts-ignore
                    document.getElementById(testTaker + "-camera").srcObject = e.streams[0];
                    this.testTakerConnections[testTaker].cameraStream = e.streams[0];
                };

                cameraConnection.onconnectionstatechange = async (e) => {
                    await this.helper.invokeMethodAsync("_onCameraConnectionStateChange", testTaker, desktopConnection.connectionState);
                };

                cameraConnection.onicecandidate = async (e) => {
                    await this.helper.invokeMethodAsync("_onCameraIceCandidate", testTaker, desktopConnection.connectionState);
                }
            });
        }
        
        public async onReceivedDesktopIceCandidate(testTaker: string, candidate: RTCIceCandidate) {
            await this.testTakerConnections[testTaker].desktopConnection.addIceCandidate(candidate);
        }
        
        public async onReceivedDesktopSdp(testTaker: string, sdp: RTCSessionDescriptionInit) {
            var conn = this.testTakerConnections[testTaker].desktopConnection;
            await conn.setRemoteDescription(sdp);
            var answer = await conn.createAnswer();
            await conn.setLocalDescription(answer);
            await this.helper.invokeMethodAsync("_onDesktopSdp", testTaker, answer);
        }

        public async onReceivedCameraIceCandidate(testTaker: string, candidate: RTCIceCandidate) {
            await this.testTakerConnections[testTaker].cameraConnection.addIceCandidate(candidate);
        }

        public async onReceivedCameraSdp(testTaker: string, sdp: RTCSessionDescriptionInit) {
            var conn = this.testTakerConnections[testTaker].cameraConnection;
            await conn.setRemoteDescription(sdp);
            var answer = await conn.createAnswer();
            await conn.setLocalDescription(answer);
            await this.helper.invokeMethodAsync("_onCameraSdp", testTaker, answer);
        }
    }
}

var webRTCClientProctor: SmartProctor.WebRTCClientProctor;

export function create(helper, testTakers: string[]) {
    if (webRTCClientProctor == null) {
        webRTCClientProctor = new SmartProctor.WebRTCClientProctor();
        webRTCClientProctor.init(helper, testTakers);
    }

    return webRTCClientProctor;
}