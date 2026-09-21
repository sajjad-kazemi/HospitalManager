using System;
using System.Collections.Generic;
using System.Text;

namespace PatientService.Application.Authentication
{
    public interface IIdentityLoginClient
    {
        Task<LoginResult?> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken);
    }
}
