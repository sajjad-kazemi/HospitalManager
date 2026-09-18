using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Authentication
{
    public interface IAccessTokenGenerator
    {
        AccessTokenResult Generate(TokenUser user);
    }
}
