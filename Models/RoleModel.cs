using System.ComponentModel.DataAnnotations;

namespace BlazorAuth.Models
{
    public class RoleModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
