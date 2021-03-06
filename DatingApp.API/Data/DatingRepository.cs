using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext context;
        public DatingRepository(DataContext context)
        {
            this.context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            this.context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            this.context.Remove(entity);
        }

        public async Task<User> GetUserAsync(int id)
        {
            User user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsersAsync(UserParams userParams)
        {
            IQueryable<User> users = this.context.Users.OrderByDescending(u => u.LastActive);

            users = users.Where(u => u.Id != userParams.UserId);

            if (!(userParams.Likees || userParams.Likers))
                users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                IEnumerable<int> userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                IEnumerable<int> userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                DateTime minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                DateTime maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                    users = users.OrderByDescending(u => u.Created);
                    break;
                    
                    default:
                    users = users.OrderByDescending(u => u.LastActive);
                    break;
                }
            }
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhotoAsync(int id)
        {
            Photo photo = await this.context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<Photo> GetMainPhotoAsync(int userId)
        {
            return await this.context.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);
        }

        public async Task<Like> GetLikeAsync(int userId, int recipientId)
        {
            return await this.context.Likes.FirstOrDefaultAsync(l => l.LikerId == userId && l.LikeeId == recipientId);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool isLiker)
        {
            User user = await this.context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (isLiker)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }    
        }

        public async Task<Message> GetMessageAsync(int id)
        {
            return await this.context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUserAsync(MessageParams messageParams)
        {
            IQueryable<Message> messages = this.context.Messages.AsQueryable();

            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.RecipientDeleted == false);
                break;
                case "Outbox":
                messages = messages.Where(m => m.SenderId == messageParams.UserId && m.SenderDeleted == false);
                break;
                default:
                messages = messages.Where(m => m.RecipientId == messageParams.UserId && m.RecipientDeleted == false && m.IsRead == false);
                break;
            }

            messages = messages.OrderByDescending(m => m.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThreadAsync(int userId, int recipientId)
        {
            List<Message> messages = await this.context.Messages
                                    .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId
                                        || m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId)
                                    .OrderByDescending(m => m.MessageSent)
                                    .ToListAsync();

            return messages;
        }
    }
}