using System.ComponentModel.DataAnnotations;

namespace BlazorAuth.Models
{
    public class PasswordInputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
