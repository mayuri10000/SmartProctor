using System;
using System.Collections.Generic;

#nullable disable

namespace SmartProctor.Server.Data.Entities
{
    public partial class ExamUser
    {
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public int? UserRole { get; set; }
    }
}
