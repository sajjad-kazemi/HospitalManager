using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Identity
{
    public static class HospitalRoles
    {
        public const string Admin = "Admin";
        public const string Receptionist = "Receptionist";
        public const string Doctor = "Doctor";
        public const string Nurse = "Nurse";

        public static readonly string[] All =
        [
            Admin,
            Receptionist,
            Doctor,
            Nurse
        ];
    }
}
