using Grpc.Core;
using HospitalManager.Contracts.Identity.V1;
using Microsoft.Extensions.Configuration;
using PatientService.Application.Employees;

namespace PatientService.Infrastructure.Employees;

public sealed class GrpcEmployeeDirectory(
    IdentityInternal.IdentityInternalClient client,
    IConfiguration configuration) : IEmployeeDirectory
{
    public async Task<EmployeeDetails?> GetByIdAsync(
        Guid id,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var serviceKey = configuration["InternalGrpc:ServiceKey"]
            ?? throw new InvalidOperationException(
                "Missing configuration: InternalGrpc:ServiceKey");

        var headers = new Metadata
        {
            { "x-service-key", serviceKey },
            { "authorization", authorizationHeader }
        };

        try
        {
            var reply = await client.GetEmployeeAsync(
                new GetEmployeeRequest { UserId = id.ToString() },
                headers: headers,
                deadline: DateTime.UtcNow.AddSeconds(5),
                cancellationToken: cancellationToken);

            return new EmployeeDetails(
                Guid.Parse(reply.UserId),
                reply.FirstName,
                reply.LastName,
                reply.Email,
                reply.Roles.ToArray(),
                DateTimeOffset.FromUnixTimeSeconds(
                    reply.CreatedAtUnixSeconds));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}