using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository repo;
        private readonly IMapper mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            this.mapper = mapper;
            this.repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            int currentLoggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            User userFromDb = await this.repo.GetUserAsync(currentLoggedInUserId);

            userParams.UserId = currentLoggedInUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromDb.Gender == "male" ? "female" : "male";
            }
            
            PagedList<User> users = await this.repo.GetUsersAsync(userParams);
            IEnumerable<ListUsersModel> usersDto = mapper.Map<IEnumerable<ListUsersModel>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersDto);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            User user = await this.repo.GetUserAsync(id);
            DetailedUserModel userDto = mapper.Map<DetailedUserModel>(user);

            return Ok(userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserModel model)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            User user = await this.repo.GetUserAsync(id);

            this.mapper.Map(model, user);
            
            if (await this.repo.SaveAllAsync())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {id} failed on save");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            Like like = await this.repo.GetLikeAsync(id, recipientId);

            if (like != null)
            {
                return BadRequest("You already liked this user");
            }
            
            if (await this.repo.GetUserAsync(recipientId) == null)
            {
                return NotFound();
            }

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            this.repo.Add<Like>(like);

            if (await this.repo.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Failed to like this user");
        }
    }
}