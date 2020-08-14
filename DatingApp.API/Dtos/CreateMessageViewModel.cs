using System;

namespace DatingApp.API.Dtos
{
    public class CreateMessageViewModel
    {
        public CreateMessageViewModel()
        {
            MessageSent = DateTime.Now;
        }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public DateTime MessageSent { get; set; }
        public string Content { get; set; }
    }
}