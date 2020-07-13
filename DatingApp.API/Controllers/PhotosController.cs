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
    [Authorize]
    [Route("api/users/{userId}/photo")]
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
        public async Task<IActionResult> AddPhotoForUser(int userId, CreatePhotoViewModel photoViewModel)
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

            photoViewModel.Url = uploadResult.Url.AbsolutePath;
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
    }
}