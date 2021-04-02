using System;

namespace SmartProctor.Shared.Requests
{
    public class UpdateExamDetailsRequestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime Duration { get; set; }
        public bool OpenBook { get; set; }
        public int MaximumTakersNum { get; set; }
    }
}