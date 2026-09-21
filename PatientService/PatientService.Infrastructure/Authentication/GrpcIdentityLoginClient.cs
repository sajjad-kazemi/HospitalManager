using Grpc.Core;
using HospitalManager.Contracts.Identity.V1;
using Microsoft.Extensions.Configuration;
using PatientService.Application.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatientService.Infrastructure.Authentication
{
    public sealed class GrpcIdentityLoginClient(
        IdentityInternal.IdentityInternalClient client,
    IConfiguration configuration) : IIdentityLoginClient)
    {
        public async Task<LoginResult?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
        {
            var serviceKey = configuration["InternalGrpc:ServiceKey"]
                ?? throw new InvalidOperationException(
                    "Missing configuration: InternalGrpc:ServiceKey");

            var headers = new Metadata
        {
            { "x-service-key", serviceKey }
        };
            try
            {
                var reply = await client.LoginAsync(
                    new LoginRequest
                    {
                        Email = email,
                        Password = password
                    },
                    headers: headers,
                    deadline: DateTime.UtcNow.AddSeconds(5),
                    cancellationToken: cancellationToken);

                return new LoginResult(
                    reply.AccessToken,
                    DateTimeOffset.FromUnixTimeSeconds(
                        reply.ExpiresAtUnixSeconds));
            }
            catch (RpcException ex)
                when (ex.StatusCode == StatusCode.Unauthenticated)
            {
                return null;
            }
        }
    }
}
