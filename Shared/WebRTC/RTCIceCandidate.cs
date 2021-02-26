namespace SmartProctor.Shared.WebRTC
{
    public class RTCIceCandidate
    {
        public string Candidate { get; set; }
        public int SdpMLineIndex { get; set; }
        public string SdpMid { get; set; }
    }
}