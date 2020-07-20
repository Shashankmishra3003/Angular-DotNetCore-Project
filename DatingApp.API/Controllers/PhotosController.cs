using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
                IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            //Settingup cloudinary with Values

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        //getting the parameter route Name for returning CreatedAtRoute
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoforUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId, true);

            //Getting the file uploaded
            var file = photoForCreationDto.File;

            //For storing the result returned from cloudary
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                //reading the file stream in memory, we need to dispose
                // the file in memory later 
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),

                        // We can transform(crop) the image before storing the image
                        Transformation = new Transformation().Width(500).Height(500)
                                            .Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            // After the response form cloudary we can Populate the DTO.

            photoForCreationDto.Url = uploadResult.Url.ToString();// Using Url in place of Uri(deprecated)
            photoForCreationDto.PublicId = uploadResult.PublicId;

            //Mapping Photo for creation DTO into our Photo model
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            //If this is the first photo then we make it as Main photo
            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            userFromRepo.Photos.Add(photo);

            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
            }

            return BadRequest("Could not Add the Photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            //Validating the user
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            //Checking if the user has the photo being asked for
            var user = await _repo.GetUser(userId, true);
            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            //If the call photo is already the main photo
            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main Photo");

            //Get current main photo
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set Photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            //Validating the user
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            //Checking if the user has the photo being asked for
            var user = await _repo.GetUser(userId, true);
            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            //If the called photo is already the main photo
            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("You cannot delete your main photo");

            if(photoFromRepo.PublicId != null)
            {
                // Creating Deletion params for deleting photos from Cloudinary
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                // returns a sting ok
                var result = _cloudinary.Destroy(deleteParams);

                //Deleting reference from SQL
                if (result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            }

            // If the photo is not in cloudinary but in  SQL
            if(photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);
            }

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to Delete the photo");
        }
    }
}
