using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validate request

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("User Already exists");

            var userToCreate = _mapper.Map<User>(userForRegisterDto);
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailDto>(createdUser);

            return CreatedAtRoute("GetUser", new { controller = "Users", id = createdUser.Id },
                                    userToReturn);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserForLogingDto userForLogingDto)
        {
            //check if user exists
            var userFromRepo = await _repo.Login(userForLogingDto.Username.ToLower(), userForLogingDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            //Generating token containing user's Id and userName

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            //Key for signing the Tokey

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config
                    .GetSection("AppSettings:Token").Value));

            //Generating signing creadentials

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //creating security token descriptor

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            //Creating JWT token handler

            var tokenHandler = new JwtSecurityTokenHandler();

            //creating a token

            var token = tokenHandler.CreateToken(tokenDescriptor);

            // returning the user data for displaying the photo on nav bar
            var user = _mapper.Map<UserForListDto>(userFromRepo);

            //returing anonymous object containing Token and a User Object 
            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });


        }
    }
}
