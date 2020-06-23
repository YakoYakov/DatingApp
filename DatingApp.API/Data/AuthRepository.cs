using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext context;

        public AuthRepository (DataContext context)
        {
            this.context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            User user = await this.context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;
            
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;    
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await this.context.AddAsync(user);
            await this.context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExists(string username)
        {
            return  await this.context.Users.AnyAsync(u => u.Username == username);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(HMACSHA512 hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
             using(HMACSHA512 hmac = new HMACSHA512(passwordSalt))
            {
                byte[] generatedPasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                if (passwordHash.Length != generatedPasswordHash.Length)
                    return false;

                for (int i = 0; i < generatedPasswordHash.Length; i++)
                {
                    if (generatedPasswordHash[i] != passwordHash[i])
                        return false;
                }
            }

            return true;
        }
    }
}