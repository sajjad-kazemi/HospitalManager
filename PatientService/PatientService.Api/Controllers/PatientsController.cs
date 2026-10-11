using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Api.Validation;
using PatientService.Application.Patients;

namespace PatientService.Api.Controllers;

[Route("api/patients")]
[Authorize(Policy = "PatientRegistration")]
public sealed class PatientsController(IPatientRegistrationService registrationService) : ApiControllerBase
{
    [HttpPost]
    [ValidateRequest(typeof(RegisterPatientRequest))]
    [ProducesResponseType<PatientDetails>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync(
        RegisterPatientRequest request, CancellationToken cancellationToken)
    {
        var patient = await registrationService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = patient.Id }, patient);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PatientDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var patient = await registrationService.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }
}
