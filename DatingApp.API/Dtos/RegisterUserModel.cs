using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class RegisterUserModel
    {
        public RegisterUserModel()
        {
            this.Created = DateTime.UtcNow;
            this.LastActive =  DateTime.UtcNow;
        }

        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} must be between {2} and {1} characters long.")]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "You must specify {0} between {2} and {1} characters long.")]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }
        
        [Required]
        public string KnownAs { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastActive { get; set; }

    }
}