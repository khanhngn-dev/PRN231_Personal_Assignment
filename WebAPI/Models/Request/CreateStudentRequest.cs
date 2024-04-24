using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Request
{
    public class CreateStudentRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Must be a valid email address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Fullname is required")]
        [MinLength(1, ErrorMessage = "Invalid name")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "Must be assigned to a student group")]
        public int GroupId { get; set; }
    }
}
