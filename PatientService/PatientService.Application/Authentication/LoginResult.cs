using System;
using System.Collections.Generic;
using System.Text;

namespace PatientService.Application.Authentication
{
    public sealed record LoginResult
    (
        string AccessToken,
        DateTimeOffset ExpiresAt
    );
}
