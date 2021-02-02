using System;
using System.Collections.Generic;

#nullable disable

namespace SmartProctor.Server.Data.Entities
{
    public partial class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
    }
}
