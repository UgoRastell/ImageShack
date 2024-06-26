using Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModel;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly RSAKeyService _rsaKeyService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LoginController(DatabaseContext dbContext, RSAKeyService rsaKeyService, IPasswordHasher<User> passwordHasher)
        {
            _dbContext = dbContext;
            _rsaKeyService = rsaKeyService;
            _rsaKeyService.EnsureKey();
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _dbContext.Users
                                       .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return Unauthorized("Invalid credentials.");
            }

            var rsaParameters = _rsaKeyService.Rsa.ExportParameters(true);
            var key = new RsaSecurityKey(rsaParameters);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = "https://localhost:7267/swagger/index.html",
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            Response.Cookies.Append("access_token", tokenString, new CookieOptions { HttpOnly = true, Secure = true });

            return Ok(new { Token = tokenString });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUserCount = await _dbContext.Users.CountAsync(u => u.Email == model.Email);
            if (existingUserCount > 0)
            {
                return BadRequest("A user with this email already exists.");
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = model.Email,
                Password = _passwordHasher.HashPassword(null, model.Password),
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully", UserId = user.UserId });
        }

        [HttpDelete("DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Images)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "User and associated images deleted successfully" });
        }
    }
}
