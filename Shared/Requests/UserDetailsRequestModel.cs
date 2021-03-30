using System.ComponentModel.DataAnnotations;

namespace SmartProctor.Shared.Requests
{
    public class UserDetailsRequestModel
    {
        [Required]
        [MaxLength(20, ErrorMessage = "Nickname should not exceed 20 characters")]
        [MinLength(5, ErrorMessage = "Nickname should be at least 5 characters")]
        public string NickName { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(30, ErrorMessage = "Email address should not exceed 30 characters")]
        public string Email { get; set; }
        
        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter a valid phone number")]
        [MaxLength(30, ErrorMessage = "Email address should not exceed 30 characters")]
        public string Phone { get; set; }
    }
}