using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         Task<bool> SaveAllAsync();
         Task<PagedList<User>> GetUsersAsync(UserParams userParams);
         Task<User> GetUserAsync(int id);
         Task<Photo> GetPhotoAsync(int id);
         Task<Photo> GetMainPhotoAsync(int userId);
         Task<Like> GetLikeAsync(int userId, int recipientId);
         Task<Message> GetMessageAsync(int id);
         Task<PagedList<Message>> GetMessagesForUserAsync(MessageParams messageParams);
         Task<IEnumerable<Message>> GetMessageThreadAsync(int userId, int recipientId);
    }
}