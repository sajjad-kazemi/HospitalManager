using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using IdentityService.Application.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Authentication
{
    public sealed class JwtAccessTokenGenerator: 
        IAccessTokenGenerator,
        IDisposable
    {
        private readonly JwtOptions _options;
        private readonly RSA _rsa;
        private readonly SigningCredentials _signingCredentials;

        public JwtAccessTokenGenerator(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            _rsa = RSA.Create();

            _rsa.ImportPkcs8PrivateKey(
                Convert.FromBase64String(_options.PrivateKey),
                out _);

            _signingCredentials = new SigningCredentials(
                new RsaSecurityKey(_rsa),
                SecurityAlgorithms.RsaSha256);
        }

        public AccessTokenResult Generate(TokenUser user)
        {
            var now = DateTimeOffset.UtcNow;
            var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            claims.AddRange(
                user.Roles.Select(role => new Claim("role", role)));

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expiresAt.UtcDateTime,
                signingCredentials: _signingCredentials);

            return new AccessTokenResult(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt);
        }

        public void Dispose()
        {
            _rsa.Dispose();
        }
    }
}
