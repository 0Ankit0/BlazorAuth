using System.ComponentModel.DataAnnotations;

namespace BlazorAuth.Models
{
    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
