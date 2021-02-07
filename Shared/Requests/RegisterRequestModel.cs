using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Requests
{
    public class RegisterRequestModel
    {
        [Required]
        [RegularExpression("^[A-Za-z0-9_]+$", ErrorMessage = "User name could only contain numbers, letters and underscore")]
        [MaxLength(20, ErrorMessage = "User name should not exceed 20 characters")]
        [MinLength(5, ErrorMessage = "User name should be at least 5 characters")]
        public string Id { get; set; }
        
        [Required]
        [MaxLength(20, ErrorMessage = "Nickname should not exceed 20 characters")]
        [MinLength(5, ErrorMessage = "Nickname should be at least 5 characters")]
        public string Nickname { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(30, ErrorMessage = "Email address should not exceed 30 characters")]
        public string Email { get; set; }
        
        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter a valid phone number")]
        [MaxLength(30, ErrorMessage = "Email address should not exceed 30 characters")]
        public string Phone { get; set; }
        
        [Required]
        [MinLength(6, ErrorMessage = "Password should be at least 6 characters")]
        public string Password { get; set; }
        
        [Required]
        [Compare("Password", ErrorMessage = "Please re-enter your password again correctly")]
        public string PasswordConfirm { get; set; }
    }
}