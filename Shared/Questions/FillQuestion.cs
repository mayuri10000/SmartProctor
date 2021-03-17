using System.Collections.Generic;

namespace SmartProctor.Shared.Questions
{
    public class FillQuestion : BaseQuestion
    {
        public IList<int> BlankPosition { get; set; }
        public IList<int> BlankType { get; set; }
    }
}