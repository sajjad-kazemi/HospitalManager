using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace PatientService.Api.Controllers
{
    [Route("api/auth")]
    public sealed class AuthController : ApiControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                UserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                Email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
                FirstName = User.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value,
                LastName = User.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value,
                Roles = User.FindAll("role").Select(claim => claim.Value)
            });
        }
    }
}
