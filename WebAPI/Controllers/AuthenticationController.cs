using BusinessObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositorty.UnitOfWork;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuthenticationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            return Ok(user);
        }
    }
}
