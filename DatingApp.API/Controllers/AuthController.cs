using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository repo;

        public AuthController (IAuthRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Register(string username, string password)
        {
            username = username.ToLower();

            if (await this.repo.UserExistsAsync(username))
                return BadRequest("Username already exists");

            User userToCreate = new User{
                Username = username
            };

            User createdUser = await this.repo.RegisterAsync(userToCreate, password);

            return StatusCode(201);    
        }
    }
}