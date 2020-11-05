using System;

namespace DatingApp.API.Dtos
{
    public class PhotoModeratorViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsApproved { get; set; }
    }
}