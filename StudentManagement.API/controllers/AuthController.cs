using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentManagement.API.Data;
using StudentManagement.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] DTOs.RegisterDto register)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new User
            {
                Username = register.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("User registered");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] DTOs.LoginDto login)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dbUser = _context.Users.FirstOrDefault(u => u.Username == login.Username);
            if (dbUser == null)
                return Unauthorized(new { message = "You are not registered" });

            if (!BCrypt.Net.BCrypt.Verify(login.Password, dbUser.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password" });

            var token = GenerateToken(dbUser);
            return Ok(new { token });
        }

        private string GenerateToken(User user)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, user.Username) };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
