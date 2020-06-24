using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class RegisterUserModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} must be between {2} and {1} characters long.")]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "You must specify {0} between {2} and {1} characters long.")]
        public string Password { get; set; }
    }
}