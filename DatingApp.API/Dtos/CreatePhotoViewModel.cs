using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Dtos
{
    public class CreatePhotoViewModel
    {
        public string Url { get; set; }
        public IFormFile File { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public string PublicId { get; set; } 

        public CreatePhotoViewModel()
        {
            this.DateAdded = DateTime.UtcNow;
        }
    }
}