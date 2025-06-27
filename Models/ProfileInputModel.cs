using System.ComponentModel.DataAnnotations;

namespace BlazorAuth.Models
{
    public class ProfileInputModel
    {
        [Required]
        public string NewUsername { get; set; } = string.Empty;
    }
}
