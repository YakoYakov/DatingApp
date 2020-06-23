using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserModel model)
        {

            // validate request

            model.Username = model.Username.ToLower();

            if (await this.repo.UserExistsAsync(model.Username))
                return BadRequest("Username already exists");

            User userToCreate = new User{
                Username = model.Username
            };

            User createdUser = await this.repo.RegisterAsync(userToCreate, model.Password);

            return StatusCode(201);    
        }
    }
}