using FluentValidation;
using Grpc.Core;
using HospitalManager.Contracts.Identity.V1;
using IdentityService.Application.Authentication;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Api.Grpc
{
    public sealed class IdentityInternalGrpcService(
        UserManager<ApplicationUser> userManager,
        IAccessTokenGenerator tokenGenerator,
        IValidator<CreateEmployeeRequest> createEmployeeValidator,
        HospitalIdentityDbContext hospitalIdentityDbContext)
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

        [Authorize(Policy = "AdminOnly")]
        public override async Task<EmployeeReply> CreateEmployee(
            CreateEmployeeRequest request,
            ServerCallContext context)
        {
            var validation = await createEmployeeValidator.ValidateAsync(
                request, context.CancellationToken);

            if (!validation.IsValid)
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Invalid employee details."));
            }

            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email ?? string.Empty,
                Email = request.Email ?? string.Empty,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            await using var transaction = await hospitalIdentityDbContext.Database.BeginTransactionAsync(context.CancellationToken);

            var userSavedResult = await userManager.CreateAsync(newUser, request.Password);

            if (!userSavedResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var duplicateUser = userSavedResult.Errors.Any(error =>
                error.Code is "DuplicateEmail" or "DuplicateUserName");
                throw new RpcException(new Status(
                    duplicateUser ? StatusCode.AlreadyExists : StatusCode.InvalidArgument,
                    duplicateUser
                    ? "An employee with this email already exists."
                    : "Employee details or password do not meet the requirements."));

            }
            if (request.Roles.Count > 0)
            {
                var addRoleResult = await userManager.AddToRolesAsync(newUser, request.Roles);

                if (!addRoleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    throw new RpcException(new Status(StatusCode.Internal, "There was an error while setting the user's role."));
                }
            }

            await transaction.CommitAsync();

            var reply = new EmployeeReply()
            {
                UserId = newUser.Id.ToString(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                CreatedAtUnixSeconds = newUser.CreatedAt.ToUnixTimeSeconds(),
            };

            reply.Roles.AddRange(request.Roles);

            return reply;

        }


        [Authorize(Policy = "AdminOnly")]
        public override async Task<EmployeeReply> AssignRole(
            AssignRoleRequest request,
            ServerCallContext context)
        {
            if (!Guid.TryParse(request.UserId, out var userId) ||
                !HospitalRoles.All.Contains(request.Role))
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    "Invalid employee ID or role."));
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new RpcException(new Status(
                    StatusCode.NotFound,
                    "Employee not found."));
            }

            if (!await userManager.IsInRoleAsync(user, request.Role))
            {
                var result = await userManager.AddToRoleAsync(user, request.Role);
                if (!result.Succeeded)
                {
                    throw new RpcException(new Status(
                        StatusCode.Internal,
                        "The role could not be assigned."));
                }
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
