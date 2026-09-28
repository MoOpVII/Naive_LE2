using BlogDataLibrary.Data;
using BlogDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private ISqlData _db;

        public LoginController(IConfiguration config, ISqlData db)
        {
            _config = config;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/api/[controller]/login")]
        public ActionResult Login([FromBody] UserLogin login)
        {
            UserModel user = _db.Authenticate(login.UserName, login.Password);

            if (user != null)
            {
                var token = GenerateToken(user);
                return Ok(token);
            }

            return NotFound("User not found");
        }

        private string GenerateToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            string userIdStr = user.Id.ToString();
            var claims = new[]
            
            {
            new Claim(ClaimTypes.NameIdentifier, userIdStr),
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/api/[controller]/register")]
        public ActionResult Register([FromBody] UserModel user)
        {
            _db.Register(user.UserName, user.FirstName, user.LastName, user.Password);
            return Ok("User registered.");
        }
    }
}
