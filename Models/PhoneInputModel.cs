using System.ComponentModel.DataAnnotations;

namespace BlazorAuth.Models
{
    public class PhoneInputModel
    {
        [Required]
        [Phone]
        public string NewPhone { get; set; } = string.Empty;
    }
}
