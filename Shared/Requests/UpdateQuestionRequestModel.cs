namespace SmartProctor.Shared.Requests
{
    public class UpdateQuestionRequestModel
    {
        public int ExamId { get; set; }
        public int QuestionNumber { get; set; }
        public string QuestionJson { get; set; }
    }
}