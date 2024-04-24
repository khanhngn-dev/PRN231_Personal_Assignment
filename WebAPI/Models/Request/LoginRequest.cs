using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(1, ErrorMessage = "Invalid username")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [MinLength(1, ErrorMessage = "Invalid password")]
        public string Password { get; set; }
    }
}
