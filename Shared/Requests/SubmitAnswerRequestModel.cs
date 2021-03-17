namespace SmartProctor.Shared.Requests
{
    public class SubmitAnswerRequestModel
    {
        public int ExamId { get; set; }
        public int QuestionNum { get; set; }
        public string AnswerJson { get; set; }
    }
}