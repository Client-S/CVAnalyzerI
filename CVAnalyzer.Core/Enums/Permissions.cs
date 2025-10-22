using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Enums
{
    public static class Permissions
    {
        //CV Management
        public const string UploadCV = "Permission.CV.Upload";
        public const string View = "Permission.CV.View";
        public const string DeleteCV = "Permission.CV.Delete";
        public const string BulkUpload = "Permission.CV.BulkUpload";

        //Student Management
        public const string ViewStudents = "Permissions.Students.View";
        public const string EditStudents = "Permissions.Students.Edit";
        public const string DeleteStudents = "Permissions.Students.Delete";

        //Reports & Analytics
        public const string GenerateReports = "Permissions.Reports.Generate";
        public const string ViewReports = "Permissions.Analytics.View";
        public const string ExportReports = "Permissions.Data.Export";

        // User Management
        public const string ManageUsers = "Permissions.Users.Manage";
        public const string ManageRoles = "Permissions.Roles.Manage";
        public const string ViewAuditLogs = "Permissions.Audit.View";
    }
}
