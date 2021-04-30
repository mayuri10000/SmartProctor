namespace SmartProctor.Shared.Requests
{
    public class SendEventRequestModel
    {
        public int ExamId { get; set; }
        public int Type { get; set; }
        public string Receipt { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
    }
}