﻿using System;
using System.Collections.Generic;

#nullable disable

namespace SmartProctor.Server.Data.Entities
{
    public partial class User
    {
        public string Id { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
    }
}
