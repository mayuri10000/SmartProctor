using System;

namespace SmartProctor.Shared.Responses
{
    public class ExamDetailsResponseModel : BaseResponseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public bool OpenBook { get; set; }
        public int MaxTakers { get; set; }
        public int QuestionCount { get; set; }
    }
}