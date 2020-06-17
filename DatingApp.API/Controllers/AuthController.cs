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

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validate request

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("User Already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
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

            //returing token as an object
            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });


        }
    }
}
