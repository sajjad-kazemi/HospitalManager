using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Application.Employees;
using PatientService.Api.Validation;

namespace PatientService.Api.Controllers;

[Route("api/employees")]
[Authorize(Policy = "AdminOnly")]
public sealed class EmployeesController(IEmployeeDirectory employeeDirectory) : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var employee = await employeeDirectory.GetByIdAsync(
            id, Request.Headers.Authorization.ToString(), cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    [ValidateRequest(typeof(CreateEmployeeRequest))]
    public async Task<IActionResult> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await employeeDirectory.CreateAsync(
            request, Request.Headers.Authorization.ToString(), cancellationToken);
        return CreatedAtAction("GetById", new { id = employee.Id }, employee);
    }

    [HttpPost("{id:guid}/roles")]
    [ValidateRequest(typeof(AssignEmployeeRoleRequest))]
    public async Task<IActionResult> AssignRoleAsync(Guid id, AssignEmployeeRoleRequest request, CancellationToken cancellationToken)
    {
        var employee = await employeeDirectory.AssignRoleAsync(
            id, request, Request.Headers.Authorization.ToString(), cancellationToken);
        return Ok(employee);
    }
}
