using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Request
{
    public class UpdateStudentRequest
    {
        [EmailAddress(ErrorMessage = "Must be a valid email address")]
        public string? Email { get; set; }
        [MinLength(1, ErrorMessage = "Invalid name")]
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? GroupId { get; set; }
    }
}
