using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using PatientService.Api.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PatientService.Api.Swagger;

public sealed class LoginRequestExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(AuthController) ||
            context.MethodInfo.Name != nameof(AuthController.Login))
        {
            return;
        }

        var content = operation.RequestBody?.Content;
        if (content is not null &&
            content.TryGetValue("application/json", out var jsonBody) &&
            jsonBody is not null)
        {
            jsonBody.Example = JsonNode.Parse("""
                {
                  "email": "admin@hospital.local",
                  "password": "hospitalAdmin123"
                }
                """);
        }
    }
}
