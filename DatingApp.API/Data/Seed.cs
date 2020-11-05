using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if(!userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                // Create some roles

                List<Role> roles = new List<Role>
                {
                    new Role{Name = "Member"},
                    new Role{Name = "Admin"},
                    new Role{Name = "Moderator"},
                    new Role{Name = "VIP"}
                };

                foreach (var role in roles)
                {
                    roleManager.CreateAsync(role).Wait();
                }

                foreach (var user in users)
                {
                   userManager.CreateAsync(user, "password").Wait();
                   userManager.AddToRoleAsync(user, "Member").Wait();
                }

                // Create Admin user

                User admin = new User{UserName = "Admin"};

                var result = userManager.CreateAsync(admin, "password").Result;

                if (result.Succeeded)
                {
                    User regAdmin = userManager.FindByNameAsync("Admin").Result;
                    userManager.AddToRolesAsync(regAdmin, new[] {"Admin", "Moderator"}).Wait();
                }
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(HMACSHA512 hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}