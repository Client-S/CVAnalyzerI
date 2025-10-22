## Step 1: Install Required NuGet Packages

### 1.1 CVAnalyzer.Web
```bash
cd CVAnalyzer.Web
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.AspNetCore.Identity.UI --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
cd ..
```

### 1.2 CVAnalyzer.Infrastructure
```bash
cd CVAnalyzer.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0
dotnet add package itext7 --version 8.0.2
dotnet add package DocumentFormat.OpenXml --version 3.0.0
dotnet add package EPPlus --version 7.0.0
cd ..
```

### 1.3 CVAnalyzer.Application
```bash
cd CVAnalyzer.Application
dotnet add package Microsoft.ML --version 3.0.1
dotnet add package FluentValidation --version 11.9.0
cd ..
```

## Step 2:
```bash
dotnet build
```
All projects should build successfully with 0 errors.
If any errors fix them before next steps.


## Step 3: Configure Database Connection
Edit CVAnalyzer.Web/appsettings.json and update the connection string if needed:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CVAnalyzerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}

## Step 4: Create and Apply Database Migrations
# Create initial migration
dotnet ef migrations add InitialCreate --project CVAnalyzer.Infrastructure --startup-project CVAnalyzer.Web

# Apply migration to database
dotnet ef database update --project CVAnalyzer.Infrastructure --startup-project CVAnalyzer.Web

## Step 5: Install LibMan globally
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Restore client-side libraries
cd CVAnalyzer.Web
libman restore

# Add new library
libman install bootstrap@5.3.2 -p cdnjs -d wwwroot/lib/bootstrap


# Run these commands in the Package Manager Console or Terminal

# 1. Create Initial Migration
# From the solution directory, run:
```bash
dotnet ef migrations add InitialCreate --project CVAnalyzer.Infrastructure --startup-project CVAnalyzer.Web --context ApplicationDbContext
```

# 2. Update Database (Apply Migration)
```bash
dotnet ef database update --project CVAnalyzer.Infrastructure --startup-project CVAnalyzer.Web --context ApplicationDbContext
```

# 3. Remove Last Migration (if needed)
```bash
dotnet ef migrations remove --project CVAnalyzer.Infrastructure --startup-project CVAnalyzer.Web --context ApplicationDbContext```
```

# 4. Drop Database (if needed for fresh start)
```bash
dotnet ef database drop --project CVAnalyzer.Infrastructure --startup-project CVAnalyzer.Web --context ApplicationDbContext --force
```

# 5. View Migration SQL Script
```bash
dotnet ef migrations script --project CVAnalyzer.Infrastructure --startup-project CVAnalyzer.Web --context ApplicationDbContext
```

# 1. Build the solution
```bash
dotnet restore
dotnet build
```
# 2. Run the application
```bash
dotnet run --project CVAnalyzer.Web
```

## Initial Login
email = "admin@cvanalyzer.com"
password = "Admin@123"

# ==============================================================================
# DEFAULT CREDENTIALS AFTER SEEDING
# ==============================================================================
# Email: admin@cvanalyzer.com
# Password: Admin@123
# Role: Admin


The application will start at:

HTTPS: https://localhost:7000
HTTP: http://localhost:5000


🐛 Common Issues & Solutions
Issue: **Migration fails**
bash
# Solution: Drop and recreate database
dotnet ef database drop --force
dotnet ef database update

Issue:** Static files not loading**

bash# Solution: Clear wwwroot and restore
rm -rf wwwroot/lib
libman restore

Issue: **Session expired**
csharp// Solution: Increase timeout in Program.cs
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12); // Increase timeout
});

Issue: ** CORS errors in API calls**
csharp// Solution: Configure CORS in Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});







