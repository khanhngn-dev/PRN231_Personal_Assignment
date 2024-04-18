using BusinessObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Repositorty.UnitOfWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public AuthenticationController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public class UserLogin
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login", Name = "Login")]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var username = userLogin.Username;
            var password = userLogin.Password;
            var user = _unitOfWork.GetRepository<UserRole>().FindByCondition(x => x.Username == username && x.Passphrase == password).FirstOrDefault();
            if (user == null)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            var token = GenerateJWTToken(user);

            return Ok(token);
        }

        private string GenerateJWTToken(UserRole user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.Role, user.UserRole1.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
