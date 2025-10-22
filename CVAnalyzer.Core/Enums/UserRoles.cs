using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Enums
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Faculty = "Faculty";
        public const string Staff = "Staff";

        public static IEnumerable<string> GetAllRoles()
        {
            return new[] { Admin, Faculty, Staff };
        }
    }
}
