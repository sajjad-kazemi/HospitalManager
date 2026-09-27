using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Application.Employees;
using StatusCodeEnum = Grpc.Core.StatusCode;

namespace PatientService.Api.Controllers
{
    [Route("api/employees")]
    [Authorize(Policy = "AdminOnly")]
    public class EmployeesController(IEmployeeDirectory employeeDirectory) : ApiControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await employeeDirectory.GetByIdAsync(
                id,
                Request.Headers.Authorization.ToString(),
                cancellationToken);

                return employee is null
                    ? NotFound()
                    : Ok(employee);
            }
            catch (RpcException exception)
            {

                if (exception.StatusCode is StatusCodeEnum.Unavailable or StatusCodeEnum.DeadlineExceeded)
                {
                    return StatusCode(503, new ProblemDetails
                    {
                        Title = "IdentityService is unavailable."
                    });
                }
                else if (exception.StatusCode == StatusCodeEnum.Unauthenticated)
                {
                    return StatusCode(401, new ProblemDetails
                    {
                        Title = "Authentication failed."
                    });
                }
                else if (exception.StatusCode == StatusCodeEnum.PermissionDenied)
                {
                    return StatusCode(403, new ProblemDetails
                    {
                        Title = "You are not allowed to use this service."
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                    {
                        Title = "an unknown problem caused an error on you request."
                    });
                }

            }
        }
    }
}
