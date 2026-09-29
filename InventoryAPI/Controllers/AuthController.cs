using InventoryAPI.Helpers;
using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISqlData _db;
        private readonly IConfiguration _config;

        public AuthController(ISqlData db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest r)
        {
            if (r == null || string.IsNullOrWhiteSpace(r.Username) ||
                string.IsNullOrWhiteSpace(r.FirstName) || string.IsNullOrWhiteSpace(r.LastName) ||
                string.IsNullOrWhiteSpace(r.Password))
                return BadRequest("Username, first name, last name, and password are required.");

            r.Username = r.Username.Trim();
            r.FirstName = r.FirstName.Trim();
            r.LastName = r.LastName.Trim();
            if (r.Username.Length > 16 || r.FirstName.Length > 50 || r.LastName.Length > 50 ||
                r.Password.Length < 8 || r.Password.Length > 128)
                return BadRequest("Username and names exceed their allowed lengths, or the password is outside the 8–128 character range.");

            //check if user exists
            var existing = _db.Authenticate(r.Username);
            if (existing != null)
                return Conflict("Username already exists.");

            string hashed = PasswordHelper.HashPassword(r.Password);
            try
            {
                _db.Register(r.Username, r.FirstName, r.LastName, hashed);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                return Conflict("Username already exists.");
            }

            return StatusCode(201);
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest l)
        {
            if (l == null || string.IsNullOrWhiteSpace(l.Username) || string.IsNullOrWhiteSpace(l.Password))
                return Unauthorized();

            var user = _db.Authenticate(l.Username.Trim());
            if (user == null)
                return Unauthorized();

            bool ok = PasswordHelper.Verify(l.Password, user.Password);
            if (!ok) return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString });
        }
    }
}
