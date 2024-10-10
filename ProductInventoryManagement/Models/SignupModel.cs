using System.ComponentModel.DataAnnotations;

namespace ProductInventoryManagement.Models
{
    public class SignupModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        public string Role { get; set; } = "User"; 
    }
}
