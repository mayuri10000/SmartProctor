namespace SmartProctor.Shared.Requests
{
    public class GetAnswerRequestModel
    {
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public int QuestionNum { get; set; }
    }
}