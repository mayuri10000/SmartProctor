using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Requests
{
    public class LoginRequestModel
    {
        [Required]
        public string UserName { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}