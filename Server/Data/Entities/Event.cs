using System;
using System.Collections.Generic;

#nullable disable

namespace SmartProctor.Server.Data.Entities
{
    public partial class Event
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string Sender { get; set; }
        public string Receipt { get; set; }
        public int Type { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
    }
}
