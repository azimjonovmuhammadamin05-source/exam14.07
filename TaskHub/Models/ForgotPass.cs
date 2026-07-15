using System.ComponentModel.DataAnnotations;

namespace TaskHub.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}