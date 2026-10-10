using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Application.Authentication;
using System.IdentityModel.Tokens.Jwt;
using PatientService.Api.Validation;

namespace PatientService.Api.Controllers;

public sealed record LoginApiRequest(string Email, string Password);

[Route("api/auth")]
public sealed class AuthController(IIdentityLoginClient identityLoginClient) : ApiControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser() => Ok(new
    {
        UserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
        Email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
        FirstName = User.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value,
        LastName = User.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value,
        Roles = User.FindAll("role").Select(claim => claim.Value)
    });

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateRequest(typeof(LoginApiRequest))]
    public async Task<IActionResult> Login(LoginApiRequest request, CancellationToken cancellationToken)
    {
        var result = await identityLoginClient.LoginAsync(request.Email, request.Password, cancellationToken);
        return result is null
            ? Unauthorized(new ProblemDetails { Title = "Invalid credentials." })
            : Ok(result);
    }
}
