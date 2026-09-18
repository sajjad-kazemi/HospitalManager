using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Authentication
{
    public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);
}
