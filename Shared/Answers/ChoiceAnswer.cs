using System.Collections.Generic;

namespace SmartProctor.Shared.Answers
{
    public class ChoiceAnswer : BaseAnswer
    {
        public IList<int> Choices { get; set; }
    }
}