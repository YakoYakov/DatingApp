using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository repository;
        private readonly IMapper mapper;
        private readonly IOptions<CloudinarySettings> cloudinarySettings;
        private readonly Cloudinary cloudinary;
        public PhotosController(IDatingRepository repository, IMapper mapper, IOptions<CloudinarySettings> cloudinarySettings)
        {
            this.cloudinarySettings = cloudinarySettings;
            this.mapper = mapper;
            this.repository = repository;

            Account account = new Account
            (
                this.cloudinarySettings.Value.CloudName,
                this.cloudinarySettings.Value.ApiKey,
                this.cloudinarySettings.Value.ApiSecret
            );

            this.cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            Photo photo = await this.repository.GetPhotoAsync(id);

            if (photo == null)
            {
                return NotFound();
            }

            ReturnPhotoViewModel photoViewModel = this.mapper.Map<ReturnPhotoViewModel>(photo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]CreatePhotoViewModel photoViewModel)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            User user = await this.repository.GetUserAsync(userId);

            IFormFile file = photoViewModel.File;

            ImageUploadResult uploadResult = new ImageUploadResult();

            // TODO: Add validation to check if the file is actually a photo
            if (file.Length > 0)
            {
                using (Stream stream = file.OpenReadStream())
                {
                    ImageUploadParams uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = this.cloudinary.Upload(uploadParams);
                }
            }

            photoViewModel.Url = uploadResult.Url.AbsoluteUri;
            photoViewModel.PublicId = uploadResult.PublicId;

            Photo photo = this.mapper.Map<Photo>(photoViewModel);

            if (!user.Photos.Any())
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await this.repository.SaveAllAsync())
            {
                ReturnPhotoViewModel returnPhotoView = this.mapper.Map<ReturnPhotoViewModel>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId ,id = photo.Id }, returnPhotoView);
            }

            return BadRequest("Could not upload image");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto (int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            User user = await this.repository.GetUserAsync(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            Photo photoToBeMain = await this.repository.GetPhotoAsync(id);

            if (photoToBeMain.IsMain)
            {
                return BadRequest("This photo is already the main photo!");
            }

            Photo currentMainPhoto = await this.repository.GetMainPhotoAsync(userId);

            if (currentMainPhoto == null)
            {
                return BadRequest("No main photo was found!");
            }

            currentMainPhoto.IsMain = false;
            photoToBeMain.IsMain = true;

            if (await this.repository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Could not set this photo to be the main photo");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
             if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            User user = await this.repository.GetUserAsync(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            Photo photoToBeDelete = await this.repository.GetPhotoAsync(id);

            if (photoToBeDelete.IsMain)
            {
                return BadRequest("You can not delete your main photo!");
            }

            if (photoToBeDelete.PublicId != null)
            {
                DeletionParams deleteParams = new DeletionParams(photoToBeDelete.PublicId);

                DeletionResult result = this.cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    this.repository.Delete(photoToBeDelete);
                }
            }
            else 
            {
                this.repository.Delete(photoToBeDelete);
            }

            if (await this.repository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Failed to delete photo!");
        }
    }
}