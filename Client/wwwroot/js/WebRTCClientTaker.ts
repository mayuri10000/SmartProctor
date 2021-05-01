namespace SmartProctor {
    class ProctorConnection {
        public desktopConnection: RTCPeerConnection;
        public cameraConnection: RTCPeerConnection;
    }

    /**
     * Typescript implementation of the WebRTC related functions in the test-taker side
     */
    export class WebRTCClientTaker {
        private proctorConnections: { [userName: string]: ProctorConnection } = {};
        private desktopStream: MediaStream;
        private cameraStream: MediaStream;
        private desktopVideoElem: Element;
        private cameraVideoElem: Element;
        private rtcConfig: RTCConfiguration;
        private cameraCanvas: HTMLCanvasElement;
        private cameraImage;
        public helper;

        public async init(helper, iceServers: string[], proctors: string[]) {
            this.helper = helper;
            if (iceServers != null) {
                this.rtcConfig = {iceServers: [{urls: iceServers}]};
            } else {
                this.rtcConfig = null;
            }

            proctors.forEach((proctor) => {
                let desktopConn = new RTCPeerConnection(this.rtcConfig);
                let cameraConn = new RTCPeerConnection(this.rtcConfig); 

                desktopConn.onicecandidate = async (e) => {
                    await helper.invokeMethodAsync("_onDesktopIceCandidate", proctor, e.candidate);
                };

                desktopConn.onconnectionstatechange = async (e) => {
                    await helper.invokeMethodAsync("_onDesktopConnectionStateChange", proctor, desktopConn.connectionState);
                };
                
                cameraConn.onicecandidate = async (e) => {
                    await helper.invokeMethodAsync("_onCameraIceCandidate", proctor, e.candidate);
                };
                
                cameraConn.onconnectionstatechange= async (e) => {
                    await helper.invokeMethodAsync("_onCameraConnectionStateChange", proctor, cameraConn.connectionState);
                };
                
                this.proctorConnections[proctor] = {
                    desktopConnection: desktopConn,
                    cameraConnection: cameraConn
                };
            });
        }

        public async obtainDesktopStream(): Promise<string> {
            // @ts-ignore
            this.desktopStream = await navigator.mediaDevices.getDisplayMedia();
            // @ts-ignore
            this.desktopStream.oninactive = async (_) => {
                await this.helper.invokeMethodAsync("_onDesktopInactivated");
            };
            return this.desktopStream.getTracks()[0].label;
        }

        public async obtainCameraStream(mjpegUrl: string): Promise<void> {
            if (this.cameraCanvas == null) {
                // @ts-ignore
                this.cameraCanvas = document.createElement<HTMLCanvasElement>("canvas");
                this.cameraCanvas.width = 858;
                this.cameraCanvas.height = 480;
            }
            
            if (this.cameraImage == null) {
                this.cameraImage = new Image();
                this.cameraImage.crossOrigin = "anonymous";
            }
            
            this.cameraImage.src = mjpegUrl;
            // @ts-ignore
            this.cameraStream = this.cameraCanvas.captureStream();
            // @ts-ignore
            this.cameraStream.oninactive = async (_) => {
                await this.helper.invokeMethodAsync("_onCameraInactivated");
            };
            window.setInterval(() => {
                this.cameraCanvas.getContext("2d").drawImage(this.cameraImage, 0, 0);
            }, 15);
        }
        

        public async startStreaming() {
            for (let proctor in this.proctorConnections) {
                let conn = this.proctorConnections[proctor];
                this.desktopStream.getTracks().forEach((track) => {
                    conn.desktopConnection.addTrack(track, this.desktopStream);
                });

                this.cameraStream.getTracks().forEach((track) => {
                    conn.cameraConnection.addTrack(track, this.cameraStream);
                });

                let desktopOffer = await conn.desktopConnection.createOffer();
                await conn.desktopConnection.setLocalDescription(desktopOffer);
                
                let cameraOffer = await conn.cameraConnection.createOffer();
                await conn.cameraConnection.setLocalDescription(cameraOffer)

                // Send the local SDP through SignalR in .NET
                await this.helper.invokeMethodAsync("_onCameraSdp", proctor, cameraOffer);
                await this.helper.invokeMethodAsync("_onDesktopSdp", proctor, desktopOffer);
            }
        }

        public async reconnectToProctor(proctor: string) {
            let conn = this.proctorConnections[proctor];

            let desktopOffer = await conn.desktopConnection.createOffer();
            await conn.desktopConnection.setLocalDescription(desktopOffer);
            
            let cameraOffer = await conn.cameraConnection.createOffer();
            await conn.cameraConnection.setLocalDescription(cameraOffer);

            // Send the local SDP through SignalR in .NET
            await this.helper.invokeMethodAsync("_onDesktopSdp", proctor, desktopOffer);
            await this.helper.invokeMethodAsync("_onCameraSdp", proctor, cameraOffer);
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

        public async receivedDesktopAnswerSDP(proctor: string, sdp: RTCSessionDescriptionInit) {
            await this.proctorConnections[proctor].desktopConnection.setRemoteDescription(sdp);
        }

        public async receivedDesktopIceCandidate(proctor: string, candidate: RTCIceCandidate) {
            await this.proctorConnections[proctor].desktopConnection.addIceCandidate(candidate);
        }

        public async receivedCameraAnswerSDP(proctor: string, sdp: RTCSessionDescriptionInit) {
            await this.proctorConnections[proctor].cameraConnection.setRemoteDescription(sdp);
        }

        public async receivedCameraIceCandidate(proctor: string, candidate: RTCIceCandidate) {
            await this.proctorConnections[proctor].cameraConnection.addIceCandidate(candidate);
        }

        public async onProctorReconnected(proctor: string) {
            let conn = this.proctorConnections[proctor];

            let desktopOffer = await conn.desktopConnection.createOffer();
            await conn.desktopConnection.setLocalDescription(desktopOffer);

            let cameraOffer = await conn.cameraConnection.createOffer();
            await conn.cameraConnection.setLocalDescription(cameraOffer);

            // Send the local SDP through SignalR in .NET
            await this.helper.invokeMethodAsync("_onDesktopSdp", proctor, desktopOffer);
            await this.helper.invokeMethodAsync("_onCameraSdp", proctor, cameraOffer);
        }
    }

}

let webRTCClientTaker: SmartProctor.WebRTCClientTaker;

export function create(helper, iceServers: string[], proctors: string[]) {
    if (webRTCClientTaker == null) {
        webRTCClientTaker = new SmartProctor.WebRTCClientTaker();
        webRTCClientTaker.init(helper, iceServers, proctors);
    }

    return webRTCClientTaker;
}