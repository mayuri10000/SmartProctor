namespace SmartProctor.Shared.Questions
{
    public class ShortAnswerQuestion : BaseQuestion
    {
        public int MaxWordCount { get; set; }
        
        public bool RichText { get; set; }
    }
}