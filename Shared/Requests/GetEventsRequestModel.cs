namespace SmartProctor.Shared.Requests
{
    public class GetEventsRequestModel
    {
        public int ExamId { get; set; }
        public string Sender { get; set; }
        public string Receipt { get; set; }
    }
}