using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            this._repo = repo;
            this._mapper = mapper;
        }

        // Dotnet core figures out where to get the parameters for this get request, since we are not
        // sending a query string, we need to explicitly tell that this parameter is FromQuery
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(currentUserId, true);

            // setting the UserId and Gender. Setting opposite gender as of user if the gender is not 
            // specified in user params
            userParams.UserId = currentUserId;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            // users is a PagedList of users, allowing to access the pagination info
            var users = await _repo.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            // Adding pagination information to response header
            Response.AddPagination(users.CurrentPage, users.PageSize,
                    users.TotalCount, users.TotalPages);
            return Ok(usersToReturn);
        }

        [HttpGet("{id}",Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == id;

            var user = await _repo.GetUser(id, isCurrentUser);
            var userToReturn = _mapper.Map<UserForDetailDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id, true);

            //maping the dating received by the post call with the user data received from server.
            _mapper.Map(userForUpdateDto, userFromRepo);

            // saving the changes and returning no content, because we are performing a PUT operation
            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user {id} failed on save");
        }

        // id is the User's own Id and recipientId is the Id of the user he likes
        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // Check if the User is already liked
            var like = await _repo.GetLike(id, recipientId);
            if (like != null)
                return BadRequest("You already Liked this User");

            // checking if recipient exists
            if (await _repo.GetUser(recipientId, false) == null)
                return NotFound();

            // creating Like object
            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId,
            };

            // not an async, adding data to memory not saving to DB.
            _repo.Add<Like>(like);

            // saving to DB
            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to like User");

        }
    }
}
