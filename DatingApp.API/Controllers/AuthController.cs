using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IConfiguration config, IMapper mapper,
                    UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _config = config;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

            // mapping to new DTO to remove password from being returned 
            var userToReturn = _mapper.Map<UserForDetailDto>(userToCreate);

            if(result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id },
                                                    userToReturn);
            }

            return BadRequest(result.Errors);          
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserForLogingDto userForLogingDto)
        {
            //check if user exists
            var user = await _userManager.FindByNameAsync(userForLogingDto.Username);

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLogingDto.Password, false);

            if(result.Succeeded)
            {
                // returning the user data for displaying the photo on nav bar
                var appUser = _mapper.Map<UserForListDto>(user);

                //returing anonymous object containing Token and a User Object 
                return Ok(new
                {
                    // returning the result property in response
                    token = GenerateJwtToken(user).Result,
                    user = appUser
                });

            }
            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            //Generating token containing user's Id and userName

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // add roles as new claims
            var roles = await _userManager.GetRolesAsync(user); // get all roles for the user

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

            return tokenHandler.WriteToken(token);
        }
    }
}
