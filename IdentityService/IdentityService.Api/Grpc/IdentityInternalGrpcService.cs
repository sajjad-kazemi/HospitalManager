using Grpc.Core;
using HospitalManager.Contracts.Identity.V1;
using IdentityService.Application.Authentication;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;

namespace IdentityService.Api.Grpc
{
    public sealed class IdentityInternalGrpcService(
        UserManager<ApplicationUser> userManager,
        IAccessTokenGenerator tokenGenerator,
        IConfiguration configuration)
        : IdentityInternal.IdentityInternalBase
    {
        public override async Task<LoginReply> Login(
            LoginRequest request,
            ServerCallContext context)
        {
            var expectedKey = configuration["InternalGrpc:ServiceKey"]
                ?? throw new InvalidOperationException(
                    "Missing configuration: InternalGrpc:ServiceKey");

            var suppliedKey = context.RequestHeaders
                .GetValue("x-service-key");

            if (string.IsNullOrEmpty(suppliedKey) ||
                suppliedKey != expectedKey)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        "Service authentication failed."));
            }

            var user = await userManager.FindByEmailAsync(
                request.Email);

            if (user is null ||
                !await userManager.CheckPasswordAsync(
                    user,
                    request.Password))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.Unauthenticated,
                        "Invalid credentials."));
            }

            var roles = new ReadOnlyCollection<string>(await userManager.GetRolesAsync(user));

            var token = tokenGenerator.Generate(
                new TokenUser(
                    user.Id,
                    user.Email ?? string.Empty,
                    user.FirstName,
                    user.LastName,
                    roles));

            return new LoginReply
            {
                AccessToken = token.AccessToken,
                ExpiresAtUnixSeconds =
                    token.ExpiresAt.ToUnixTimeSeconds()
            };
        }
    }
}
