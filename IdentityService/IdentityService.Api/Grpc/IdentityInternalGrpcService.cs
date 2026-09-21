using Grpc.Core;
using HospitalManager.Contracts.Identity.V1;
using IdentityService.Application.Authentication;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;

namespace IdentityService.Api.Grpc
{
    public sealed class IdentityInternalGrpcService(
        UserManager<ApplicationUser> userManager,
        IAccessTokenGenerator tokenGenerator)
        : IdentityInternal.IdentityInternalBase
    {
        public override async Task<LoginReply> Login(
            LoginRequest request,
            ServerCallContext context)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null || await userManager.IsLockedOutAsync(user))
            {
                throw new RpcException(new Status(
                    StatusCode.Unauthenticated,
                    "Invalid credentials."));
            }

            if (!await userManager.CheckPasswordAsync(user, request.Password))
            {
                var failedResult = await userManager.AccessFailedAsync(user);

                if (!failedResult.Succeeded)
                {
                    throw new RpcException(new Status(
                        StatusCode.Internal,
                        "Authentication could not be completed."));
                }

                throw new RpcException(new Status(
                    StatusCode.Unauthenticated,
                    "Invalid credentials."));
            }

            var resetResult = await userManager.ResetAccessFailedCountAsync(user);

            if (!resetResult.Succeeded)
            {
                throw new RpcException(new Status(
                    StatusCode.Internal,
                    "Authentication could not be completed."));
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

        [Authorize(Policy = "AdminOnly")]
        public override async Task<EmployeeReply> GetEmployee(
            GetEmployeeRequest request,
            ServerCallContext context)
        {
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Invalid user ID."));
            }

            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                throw new RpcException(new Status(
                    StatusCode.NotFound,
                    "Employee not found."));
            }

            var roles = await userManager.GetRolesAsync(user);

            var reply = new EmployeeReply
            {
                UserId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                CreatedAtUnixSeconds = user.CreatedAt.ToUnixTimeSeconds()
            };

            reply.Roles.AddRange(roles);
            return reply;
        }

    }
}
