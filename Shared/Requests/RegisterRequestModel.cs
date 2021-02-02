using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Requests
{
    public class RegisterRequestModel
    {
        [Required(ErrorMessage = "User name is required")]
        
        public string Id { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        
        [Compare("Password", ErrorMessage = "Please re-enter your password again correctly")]
        public string PasswordConfirm { get; set; }
    }
}