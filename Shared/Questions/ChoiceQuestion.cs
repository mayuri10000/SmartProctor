using System.Collections.Generic;

namespace SmartProctor.Shared.Questions
{
    public class ChoiceQuestion : BaseQuestion
    {
        public bool MultiChoice { get; set; }
        public IList<string> Choices { get; set; }
    }
}