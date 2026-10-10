using Grpc.Core;
using HospitalManager.Contracts.Identity.V1;
using Microsoft.Extensions.Configuration;
using PatientService.Application.Employees;
using CreateEmployeeInput = PatientService.Application.Employees.CreateEmployeeRequest;
using GrpcCreateEmployeeRequest = HospitalManager.Contracts.Identity.V1.CreateEmployeeRequest;

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
        var headers = CreateHeaders(authorizationHeader);

        try
        {
            var reply = await client.GetEmployeeAsync(
                new GetEmployeeRequest { UserId = id.ToString() },
                headers: headers,
                deadline: DateTime.UtcNow.AddSeconds(5),
                cancellationToken: cancellationToken);

            return MapEmployee(reply);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<EmployeeDetails> CreateAsync(
        CreateEmployeeInput request,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var grpcRequest = new GrpcCreateEmployeeRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password
        };
        grpcRequest.Roles.AddRange(request.Roles);

        var reply = await client.CreateEmployeeAsync(
            grpcRequest,
            headers: CreateHeaders(authorizationHeader),
            deadline: DateTime.UtcNow.AddSeconds(5),
            cancellationToken: cancellationToken);

        return MapEmployee(reply);
    }

    public async Task<EmployeeDetails> AssignRoleAsync(
        Guid employeeId,
        AssignEmployeeRoleRequest request,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var reply = await client.AssignRoleAsync(
            new AssignRoleRequest
            {
                UserId = employeeId.ToString(),
                Role = request.Role
            },
            headers: CreateHeaders(authorizationHeader),
            deadline: DateTime.UtcNow.AddSeconds(5),
            cancellationToken: cancellationToken);

        return MapEmployee(reply);
    }

    private Metadata CreateHeaders(string authorizationHeader)
    {
        var serviceKey = configuration["InternalGrpc:ServiceKey"]
            ?? throw new InvalidOperationException(
                "Missing configuration: InternalGrpc:ServiceKey");

        var headers = new Metadata
        {
            { "x-service-key", serviceKey },
            { "authorization", authorizationHeader }
        };
        return headers;
    }

    private static EmployeeDetails MapEmployee(EmployeeReply reply) => new(
        Guid.Parse(reply.UserId),
        reply.FirstName,
        reply.LastName,
        reply.Email,
        reply.Roles.ToArray(),
        DateTimeOffset.FromUnixTimeSeconds(reply.CreatedAtUnixSeconds));
}
