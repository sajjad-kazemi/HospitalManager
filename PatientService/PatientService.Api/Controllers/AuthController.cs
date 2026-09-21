using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Application.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace PatientService.Api.Controllers
{
    public sealed record LoginApiRequest(
       string Email,
       string Password);

    [Route("api/auth")]
    public sealed class AuthController(
        IIdentityLoginClient _identityLoginClient) : ApiControllerBase
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


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
    LoginApiRequest request,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Email and password are required."
                });
            }

            try
            {
                var result = await _identityLoginClient.LoginAsync(
                    request.Email,
                    request.Password,
                    cancellationToken);

                return result is null
                    ? Unauthorized(new ProblemDetails
                    {
                        Title = "Invalid credentials."
                    })
                    : Ok(result);
            }
            catch (RpcException)
            {
                return StatusCode(503, new ProblemDetails
                {
                    Title = "IdentityService is unavailable."
                });
            }
        }

    }
}