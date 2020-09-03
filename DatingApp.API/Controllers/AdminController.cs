using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext context;
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly Cloudinary cloudinary;
        private readonly IOptions<CloudinarySettings> cloudinarySettings;
        public AdminController(DataContext context, UserManager<User> userManager, IMapper mapper, IOptions<CloudinarySettings> cloudinarySettings)
        {
            this.cloudinarySettings = cloudinarySettings;
            this.mapper = mapper;
            this.userManager = userManager;
            this.context = context;

            Account account = new Account
            (
                this.cloudinarySettings.Value.CloudName,
                this.cloudinarySettings.Value.ApiKey,
                this.cloudinarySettings.Value.ApiSecret
            );

            this.cloudinary = new Cloudinary(account);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var usersWithRoles = await this.context.Users
                .OrderBy(user => user.UserName)
                .Select(user => new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = (from userRole in user.UserRoles
                             join role in this.context.Roles
                             on userRole.RoleId
                             equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();


            return Ok(usersWithRoles);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, EditRoleViewModel rolesViewModel)
        {
            User user = await this.userManager.FindByNameAsync(userName);

            if (user == null)
                return NotFound("User can not be found");

            IList<string> userRoles = await this.userManager.GetRolesAsync(user);

            string[] selectedRoles = rolesViewModel.RoleNames;

            selectedRoles = selectedRoles ?? new string[] { };

            IdentityResult addedRolesResult = await this.userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!addedRolesResult.Succeeded)
                return BadRequest($"Failed to add to roles user {userName}");

            IdentityResult removedRolesResult = await this.userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!removedRolesResult.Succeeded)
                return BadRequest($"Failed to remove the roles for {userName}");

            return Ok(await this.userManager.GetRolesAsync(user));
        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            List<Photo> unapprovedPhotosFromDb = await this.context.Photos.Where(p => p.isApproved == false).ToListAsync();

            var unapprovedPhotosViewModel = this.mapper.Map<PhotoModeratorViewModel[]>(unapprovedPhotosFromDb);

            return Ok(unapprovedPhotosViewModel);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpDelete("photosForModeration/{id}")]
        public async Task<IActionResult> DisApprovePhoto(int id)
        {

            Photo photoToBeDelete = await this.context.Photos.FindAsync(id);

            if (photoToBeDelete == null)
            {
                return NotFound("Photo not found");
            }

            if (photoToBeDelete.PublicId != null)
            {
                DeletionParams deleteParams = new DeletionParams(photoToBeDelete.PublicId);

                DeletionResult result = this.cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    this.context.Photos.Remove(photoToBeDelete);
                }
            }
            else
            {
                this.context.Photos.Remove(photoToBeDelete);
            }
         
            try
            {
                await this.context.SaveChangesAsync();

                return Ok(); 
            }
            catch (DbException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}