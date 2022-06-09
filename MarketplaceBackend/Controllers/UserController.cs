﻿using MarketplaceBackend.Data;
using MarketplaceBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MarketplaceBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public UserController(DataContext dataContext, IConfiguration configuration)
        {
            _context = dataContext;
            _config = configuration;
        }

        [HttpPost("/login")]
        public IActionResult Login([FromBody] UserLoginDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var identity = GetIdentity(user.Email, user.Password);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid email or password." });
            }

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                    issuer: _config.GetValue<string>("Jwt:Issuer"),
                    audience: _config.GetValue<string>("Jwt:Audience"),
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromSeconds(_config.GetValue<int>("Jwt:Lifetime"))),
                    signingCredentials: new(
                            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetValue<string>("Jwt:Key"))),
                            SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                email = identity.Name
            };
            return Ok(response);
        }
        private ClaimsIdentity GetIdentity(string email, string password)
        {
            User user = _context.Users.ToList().FirstOrDefault(x => x.Email == email && PasswordEncoder.ValidatePassword(password, x.Hash, x.Salt));
            if (user != null)
            {
                List<Claim> claims = new(){
                    new("user_id", user.Id.ToString()),
                    new("fname", user.FirstName),
                    new("lname", user.LastName),
                    new(ClaimsIdentity.DefaultNameClaimType, user.Email),
                    new(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString())
                };
                ClaimsIdentity claimsIdentity = new(claims, "Token",
                    ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }
            return null;
        }
        
        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto user)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(_context.Users.Any(e=>e.Email==user.Email))
            {
                return BadRequest(new { errorText = "User with this email already exists."});
            }

            var salt = PasswordEncoder.GenerateSalt();            

            _context.Users.Add(new() { FirstName=user.FirstName, LastName=user.LastName, Email=user.Email, Hash= PasswordEncoder.HashPassword(user.Password, salt), Salt=Convert.ToBase64String(salt), Role=Role.User});
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
