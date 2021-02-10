// This file contains some JS functions that use the 'new' operator to
// create instance of JS objects as 'new' operator is not supported in JSInterop

function newRTCPeerConnection(conf) {
    return new RTCPeerConnection(conf);
}

function newRTCSessionDescription(sdp) {
    return new RTCSessionDescription(sdp);
}

function newRTCIceCandidate(a) {
    return new RTCIceCandidate(a);
}
