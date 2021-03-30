using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Requests
{
    public class ModifyPasswordRequestModel 
    {
        [Required]
        public string OldPassword { get; set; }
        
        [Required]
        [MinLength(6, ErrorMessage = "Password should be at least 6 characters")]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword", ErrorMessage = "Please re-enter your password again correctly")]
        public string PasswordConfirm { get; set; }
    }
}