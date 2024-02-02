using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ReadVideo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        const string symmetricJwtKey = "aTGeUGu2fBQstsUkLFryni51LpCxl0Mqg7pLGTPvt6c=";
        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("google")]
        public async Task<IActionResult> Google([FromBody] GoogleTokenDto tokenDto)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(tokenDto.AccessToken, new GoogleJsonWebSignature.ValidationSettings());

                // Now you can use payload to create your own user and JWT token
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, payload.Subject),
                    new Claim(ClaimTypes.Email, payload.Email),
                    // Add other claims as needed
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[symmetricJwtKey]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.Now.AddDays(1);

                var token = new JwtSecurityToken(
                    "Jwt:Issuer",
                    "Jwt:Audience",
                    claims,
                    expires: expiry,
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = expiry
                });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized();
            }
        }
    }

    public class GoogleTokenDto
    {
        public string AccessToken { get; set; }
    }
}
