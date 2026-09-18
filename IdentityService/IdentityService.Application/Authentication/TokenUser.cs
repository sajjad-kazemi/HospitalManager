using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Authentication
{
    public sealed record TokenUser(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles);
}
