using System.ComponentModel.DataAnnotations;

namespace blogapp.ViewModels
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
