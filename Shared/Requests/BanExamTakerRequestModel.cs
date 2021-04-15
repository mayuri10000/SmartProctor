namespace SmartProctor.Shared.Requests
{
    public class BanExamTakerRequestModel
    {
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public string Reason { get; set; }
    }
}