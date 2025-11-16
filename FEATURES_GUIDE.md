# CVAnalyzer - Complete Features Guide & Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [System Architecture](#system-architecture)
3. [Installation & Setup](#installation--setup)
4. [Core Features](#core-features)
5. [User Guide](#user-guide)
6. [Administrative Features](#administrative-features)
7. [Export & Reporting](#export--reporting)
8. [Machine Learning Features](#machine-learning-features)
9. [API Reference](#api-reference)
10. [Configuration Options](#configuration-options)
11. [Technical Details](#technical-details)

---

## Project Overview

**CVAnalyzer** is an intelligent CV/Resume analysis and management system built with ASP.NET Core 8.0. It provides automated CV parsing, student profile management, skill-based clustering using machine learning, and comprehensive analytics and reporting capabilities.

### Key Capabilities
- Automated CV parsing from PDF and DOCX formats
- Student profile and skill management
- ML-powered clustering (K-Means & DBSCAN)
- Semantic skill matching and similarity analysis
- Role-based access control
- Comprehensive export and reporting
- Background job processing for intensive operations
- Audit logging for compliance

---

## System Architecture

CVAnalyzer follows **Clean Architecture** principles with clear separation of concerns across four distinct layers:

### Layer Structure

```
StudentCVAnalyzer.sln
├── CVAnalyzer.Core/          # Domain Layer
│   ├── Entities/             # Domain models
│   ├── Interfaces/           # Repository contracts
│   └── Enums/                # Business enumerations
│
├── CVAnalyzer.Application/   # Application Layer
│   ├── DTOs/                 # Data transfer objects
│   ├── Services/             # Service interfaces
│   └── Models/               # Application models
│
├── CVAnalyzer.Infrastructure/ # Infrastructure Layer
│   ├── Data/                 # Database context & migrations
│   ├── Repositories/         # Data access implementations
│   ├── Services/             # Service implementations
│   └── Queue/                # Background job infrastructure
│
└── CVAnalyzer.Web/           # Presentation Layer
    ├── Controllers/          # MVC & API controllers
    ├── Views/                # Razor views
    ├── Pages/                # Razor pages
    ├── Middleware/           # Custom middleware
    └── wwwroot/              # Static files & uploads
```

### Design Patterns Implemented
- **Repository Pattern**: Generic repository with specialized implementations
- **Unit of Work Pattern**: Transaction management across repositories
- **Service Layer Pattern**: Business logic encapsulation
- **Dependency Injection**: Constructor-based DI throughout
- **DTO Pattern**: Clean data transfer between layers
- **Background Queue Pattern**: Asynchronous task processing
- **Middleware Pattern**: Custom exception handling
- **Factory Pattern**: ML.NET context creation

---

## Installation & Setup

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server or LocalDB
- Visual Studio 2022 or VS Code
- Node.js (for frontend assets, if needed)

### Step 1: Clone the Repository
```bash
git clone https://github.com/Client-S/CVAnalyzerI.git
cd CVAnalyzerI
```

### Step 2: Configure Database Connection
Edit `CVAnalyzer.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CVAnalyzerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

For SQL Server, use:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=CVAnalyzerDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true"
```

### Step 3: Apply Database Migrations
```bash
cd CVAnalyzer.Web
dotnet ef database update
```

### Step 4: Configure Application Settings
Review and update `appsettings.json`:
- JWT secret key (change for production)
- File upload limits
- Clustering configuration
- Storage provider settings

### Step 5: Build and Run
```bash
dotnet build
dotnet run --project CVAnalyzer.Web
```

The application will be available at `https://localhost:7000` (default).

### Step 6: Initial Login
Default admin credentials:
- **Email**: admin@cvanalyzer.com
- **Password**: Admin@123

**Important**: Change the default password immediately after first login.

---

## Core Features

### 1. CV Upload & Processing

#### **Single CV Upload**
Allows uploading individual CV files with manual metadata entry.

**Features**:
- Upload PDF or DOCX files (max 10MB)
- Manual entry of student information (ID, Name, Email, Phone)
- Automatic text extraction and parsing
- Skills detection from predefined skill database
- Work experience extraction
- Real-time processing status

**How It Works**:
1. User uploads CV file through web interface
2. System validates file type and size
3. File is stored in `wwwroot/uploads/cvs/[StudentId]/`
4. CV text is extracted using iText7 (PDF) or OpenXML (DOCX)
5. Regex patterns extract student information
6. Skills are matched against predefined skill list
7. Student profile is created or updated in database
8. Processing results are displayed to user

**Location**:
- Controller: `CVAnalyzer.Web/Controllers/CVController.cs:18`
- Service: `CVAnalyzer.Infrastructure/Services/CVProcessingService.cs:35`

#### **Bulk CV Upload**
Process multiple CVs simultaneously with automatic metadata extraction.

**Features**:
- Upload multiple CV files at once
- Automatic student ID extraction
- Batch processing with transaction support
- Detailed success/failure reporting
- Skip duplicates or update existing records

**How It Works**:
1. User selects multiple CV files
2. System processes each file in sequence
3. Student ID is automatically extracted from CV content
4. If extraction fails, filename is used as fallback
5. Each CV is parsed and student record created
6. Results summary shows successes, failures, and errors
7. All changes are committed in a single transaction

**Location**:
- Controller: `CVAnalyzer.Web/Controllers/CVController.cs:45`
- Service: `CVAnalyzer.Infrastructure/Services/CVProcessingService.cs:120`

---

### 2. Intelligent CV Parsing

The CV parsing engine uses advanced regex patterns and NLP techniques to extract structured information from unstructured CV documents.

#### **Information Extraction Capabilities**

**Student ID Detection**:
- Patterns: `Student ID:`, `ID:`, `Roll No:`, `USN:`
- Supports various formats: alphanumeric, numeric, hyphenated
- Fallback to filename if not found

**Name Extraction**:
- Extracts from CV header (first 500 characters)
- Handles formats: "Name: John Doe", "JOHN DOE" (all caps)
- Validates name length and format

**Email Detection**:
- Standard email regex pattern
- Validates domain format
- Extracts first valid email found

**Phone Number Extraction**:
- Multiple format support:
  - US: (123) 456-7890, 123-456-7890
  - International: +91-1234567890
  - Standard: 1234567890
- Validates number length (10-15 digits)

**Skills Extraction**:
- Matches against predefined skill database
- Case-insensitive whole-word matching
- Synonym support (e.g., "JS" = "JavaScript")
- Confidence score: 0.8 (basic implementation)
- Stores extracted context for verification

**Work Experience Parsing**:
- Identifies experience section keywords
- Extracts company names
- Detects job titles/positions
- Parses dates and duration
- Captures job descriptions

**Supported File Formats**:
- **PDF**: Uses iText7 library for text extraction
- **DOCX**: Uses DocumentFormat.OpenXml for parsing
- **TXT**: Direct text reading (planned)

**Location**:
- Parser Service: `CVAnalyzer.Infrastructure/Services/CVParserService.cs:20`
- Processing Logic: `CVAnalyzer.Infrastructure/Services/CVProcessingService.cs:220`

---

### 3. Student Management

Complete CRUD operations for student profiles with advanced search and filtering.

#### **Student Profile Management**

**Create Student**:
- Manual student creation
- Required: Student ID (unique), Name, Email
- Optional: Phone number
- Automatic timestamp tracking

**View Student Details**:
- Complete profile information
- Skills list with proficiency levels
- Work experience timeline
- All uploaded CVs with download links
- Processing history and status

**Update Student**:
- Edit basic information
- Cannot change Student ID (immutable)
- Update email and phone
- Modification tracking (ModifiedDate)

**Delete Student**:
- Admin-only operation
- Cascading delete of related records:
  - Student skills
  - Work experiences
  - CV documents (files and database records)
  - Cluster memberships
- Audit logging of deletion

**Search & Filter**:
- **Full-text search**: Search by name, email, or student ID
- **Skill-based filter**: Find students with specific skills
- **Advanced queries**: Combine multiple criteria
- Eager loading for performance optimization

**Location**:
- Controller: `CVAnalyzer.Web/Controllers/StudentController.cs`
- Service: `CVAnalyzer.Infrastructure/Services/StudentService.cs`
- Repository: `CVAnalyzer.Infrastructure/Repositories/StudentRepository.cs`

---

### 4. Clustering & Analytics

Advanced clustering capabilities using both traditional algorithms and machine learning.

#### **Simple Skill-Based Clustering**

**Description**: Creates clusters based on skill similarity using Jaccard coefficient.

**How It Works**:
1. User specifies minimum and maximum skill counts
2. System filters students within skill range
3. Calculates pairwise Jaccard similarity
4. Groups students with similarity > threshold (0.3)
5. Creates cluster with similar students

**Use Cases**:
- Quick grouping by skill count
- Finding students with similar skill sets
- Team formation for projects

**Location**: `CVAnalyzer.Infrastructure/Services/ClusteringService.cs:40`

#### **K-Means Clustering (ML-Powered)**

**Description**: Machine learning clustering using ML.NET K-Means algorithm.

**Features**:
- Automatic feature engineering
- Binary skill vectorization
- Silhouette score calculation
- Optimal cluster count determination

**How It Works**:
1. User specifies number of clusters (k)
2. System loads all students with skills
3. Creates unique skill list across all students
4. Generates binary feature vectors (1 = has skill, 0 = doesn't)
5. Trains K-Means model with ML.NET
6. Predicts cluster assignments for each student
7. Calculates similarity scores based on cluster distances
8. Stores cluster with members and scores

**Parameters**:
- `k`: Number of clusters (default: 3)
- `maxIterations`: ML training iterations (default: 100)

**Location**: `CVAnalyzer.Infrastructure/Services/MLClusteringService.cs:45`

#### **DBSCAN Clustering (Density-Based)**

**Description**: Density-based clustering that discovers natural groupings without predefined cluster count.

**Features**:
- Automatic cluster count detection
- Noise point identification
- Outlier detection
- No spherical cluster assumption

**How It Works**:
1. User specifies epsilon (neighborhood radius) and minPoints (minimum cluster size)
2. System creates skill vectors for all students
3. Calculates pairwise Euclidean distances
4. Identifies core points (points with ≥ minPoints neighbors within epsilon)
5. Expands clusters from core points
6. Marks noise points as cluster -1 (outliers)
7. Stores clusters with density-based groupings

**Parameters**:
- `epsilon`: Maximum distance for neighborhood (default: 0.5)
- `minPoints`: Minimum points for cluster (default: 2)

**Use Cases**:
- Discovering natural groupings
- Identifying outlier profiles
- Variable-size cluster detection

**Location**: `CVAnalyzer.Infrastructure/Services/MLClusteringService.cs:180`

#### **Find Similar Students**

**Description**: Finds students with similar skill profiles using Jaccard similarity.

**Features**:
- Configurable similarity threshold
- Ranked results by similarity score
- Identifies matching skills
- Supports team building

**How It Works**:
1. User selects a student
2. System retrieves student's skills
3. Calculates Jaccard similarity with all other students
4. Filters students above threshold (default: 0.3)
5. Returns ranked list with matching skills

**Location**: `CVAnalyzer.Infrastructure/Services/ClusteringService.cs:120`

---

### 5. Semantic Skill Matching

Advanced skill matching using synonym dictionaries and fuzzy string matching.

#### **Skill Synonym Dictionary**

Predefined mappings for common skill variations:
- **JavaScript**: JS, ECMAScript, javascript
- **Python**: python, py
- **C#**: CSharp, C Sharp, csharp
- **AWS**: Amazon Web Services, aws
- **React**: ReactJS, React.js
- **Node.js**: NodeJS, Node
- And many more...

#### **Similarity Calculation Methods**

**Exact Match**: 1.0 similarity
- Case-insensitive comparison
- Normalized whitespace

**Synonym Match**: 0.9 similarity
- Checks synonym dictionary
- Bidirectional matching

**Partial Match**: 0.7 similarity
- One skill contains the other
- Handles abbreviations

**Levenshtein Distance**: 0.0-1.0 similarity
- Edit distance calculation
- Normalized by string length
- Handles typos and variations

**Location**: `CVAnalyzer.Infrastructure/Services/MLClusteringService.cs:320`

---

### 6. User Management & Authentication

Comprehensive user management with role-based access control.

#### **Authentication Methods**

**Cookie-Based Authentication**:
- For web application access
- Sliding expiration (8 hours)
- HttpOnly cookies for security
- Automatic renewal on activity

**JWT Token Authentication**:
- For API access
- Configurable expiry (default: 60 minutes)
- Bearer token scheme
- Includes user claims (ID, email, roles)

#### **User Roles**

**Admin**:
- Full system access
- User management
- Student deletion
- Cluster deletion
- System configuration

**Faculty**:
- Student management (view, create, edit)
- CV upload and processing
- Cluster creation and viewing
- Export and reporting

**Staff**:
- Student viewing
- CV viewing
- Basic reporting

#### **Security Features**

**Password Requirements**:
- Minimum 8 characters
- At least one digit
- At least one lowercase letter
- At least one uppercase letter
- At least one non-alphanumeric character

**Account Lockout**:
- 5 failed login attempts
- 15-minute lockout duration
- Automatic unlock after timeout

**Password Management**:
- Change password functionality
- Password history (prevents reuse)
- Admin password reset capability

**Location**:
- Auth Service: `CVAnalyzer.Infrastructure/Services/AuthService.cs`
- User Management: `CVAnalyzer.Infrastructure/Services/UserManagementService.cs`
- Account Controller: `CVAnalyzer.Web/Controllers/AccountController.cs`

---

### 7. Audit Logging

Comprehensive audit trail for compliance and monitoring.

#### **Logged Actions**
- Student creation, update, deletion
- CV upload, processing, deletion
- Cluster creation, deletion
- User login, logout, password changes
- Export operations
- Admin actions

#### **Audit Information Captured**
- **User ID**: Who performed the action
- **Action**: What was done
- **Entity Type**: What was affected (Student, CV, Cluster, etc.)
- **Entity ID**: Specific record ID
- **Details**: JSON object with additional context
- **IP Address**: Source of the request
- **Timestamp**: When the action occurred (indexed for performance)

#### **Audit Queries**
- View logs by user
- View logs by action type
- View logs by date range
- View logs for specific entity

**Location**:
- Service: `CVAnalyzer.Infrastructure/Services/AuditService.cs`
- Entity: `CVAnalyzer.Core/Entities/AuditLog.cs`

---

## User Guide

### Getting Started

#### First-Time Login
1. Navigate to `https://localhost:7000` (or your configured URL)
2. Click "Login" in the navigation bar
3. Enter default credentials:
   - Email: admin@cvanalyzer.com
   - Password: Admin@123
4. You will be prompted to change your password
5. After password change, you're ready to use the system

---

### Uploading CVs

#### Single CV Upload

1. **Navigate to Upload Page**:
   - Click "CVs" → "Upload Single CV" in the navigation menu

2. **Fill Student Information**:
   - Student ID (required, unique)
   - Name (required)
   - Email (required, valid format)
   - Phone (optional)

3. **Select CV File**:
   - Click "Choose File"
   - Select PDF or DOCX file (max 10MB)
   - Supported formats displayed on page

4. **Upload**:
   - Click "Upload CV" button
   - Wait for processing (progress indicator shown)
   - View results on confirmation page

5. **Review Results**:
   - Extracted skills listed
   - Detected work experience shown
   - Any errors or warnings displayed
   - Option to view student details

#### Bulk CV Upload

1. **Navigate to Bulk Upload**:
   - Click "CVs" → "Bulk Upload" in menu

2. **Select Multiple Files**:
   - Click "Choose Files"
   - Select multiple CV files
   - All files must be PDF or DOCX
   - Each file should contain student ID

3. **Upload**:
   - Click "Upload CVs" button
   - System processes files sequentially
   - Progress shown for each file

4. **Review Summary**:
   - Total files processed
   - Successful uploads count
   - Failed uploads with error details
   - Download error log if needed

**Tips**:
- Name files with student IDs for better tracking
- Ensure CVs contain student ID in text
- If student ID not found, filename is used
- Duplicate student IDs will update existing records

---

### Managing Students

#### Viewing Student List

1. **Navigate to Students**:
   - Click "Students" in main navigation
   - View paginated list of all students

2. **Student List Information**:
   - Student ID
   - Name
   - Email
   - Phone
   - Number of skills
   - Number of CVs uploaded

3. **Actions Available**:
   - **View Details**: Click student name or "Details" button
   - **Edit**: Click "Edit" button (Faculty/Admin only)
   - **Delete**: Click "Delete" button (Admin only)

#### Viewing Student Details

1. **Access Details Page**:
   - Click student name from list
   - Or click "Details" button

2. **Information Displayed**:
   - **Basic Info**: Student ID, Name, Email, Phone
   - **Skills**: List of all skills with proficiency
   - **Work Experience**: Timeline of past positions
   - **Uploaded CVs**: All CV documents with:
     - File name
     - Upload date
     - File size
     - Processing status
     - Download link

3. **Available Actions**:
   - Download any CV
   - Delete specific CV (removes file and record)
   - Edit student information
   - Find similar students (clustering)

#### Searching Students

1. **Use Search Bar**:
   - Located at top of student list
   - Type search term
   - Press Enter or click "Search"

2. **Search Scope**:
   - Student ID (partial or full)
   - Name (first or last)
   - Email address

3. **View Results**:
   - Matching students displayed
   - Click "Clear" to reset search

#### Filtering by Skill

1. **Access Skill Filter**:
   - Click "Filter by Skill" dropdown
   - Or use skill links from reports

2. **Select Skill**:
   - Choose from dropdown list
   - Shows all students with that skill

3. **View Results**:
   - Students with selected skill
   - Skill proficiency shown
   - Option to export filtered list

---

### Creating Clusters

#### Simple Cluster

1. **Navigate to Clustering**:
   - Click "Clusters" → "Create Simple Cluster"

2. **Configure Parameters**:
   - **Cluster Name**: Descriptive name
   - **Minimum Skills**: Minimum skill count (e.g., 3)
   - **Maximum Skills**: Maximum skill count (e.g., 10)
   - **Description**: Optional cluster purpose

3. **Create Cluster**:
   - Click "Create Cluster"
   - System calculates Jaccard similarity
   - Groups similar students

4. **View Results**:
   - Cluster details page
   - List of members
   - Matching skills highlighted
   - Similarity scores

#### K-Means ML Cluster

1. **Navigate to ML Clustering**:
   - Click "Clusters" → "Create ML Cluster"

2. **Configure K-Means**:
   - **Cluster Name**: Descriptive name
   - **Number of Clusters (k)**: Desired cluster count (e.g., 3-5)
   - **Description**: Optional purpose

3. **Create Cluster**:
   - Click "Create K-Means Cluster"
   - ML algorithm trains on student data
   - Students assigned to optimal clusters

4. **View Results**:
   - Multiple clusters created
   - Silhouette scores shown (quality metric)
   - Members evenly distributed
   - Common skills per cluster

**When to Use K-Means**:
- Know desired number of groups
- Want balanced cluster sizes
- Need clear cluster assignments
- Large dataset (50+ students)

#### DBSCAN ML Cluster

1. **Navigate to ML Clustering**:
   - Same as K-Means

2. **Configure DBSCAN**:
   - **Cluster Name**: Descriptive name
   - **Epsilon**: Neighborhood radius (0.3-0.7 typical)
   - **Minimum Points**: Min cluster size (2-5 typical)
   - **Description**: Optional purpose

3. **Create Cluster**:
   - Click "Create DBSCAN Cluster"
   - Algorithm discovers natural groupings
   - Noise points identified

4. **View Results**:
   - Variable number of clusters (discovered automatically)
   - Outliers marked as "Noise" cluster
   - Density-based groupings
   - Core points vs. border points

**When to Use DBSCAN**:
- Don't know optimal cluster count
- Want to identify outliers
- Expect irregular cluster shapes
- Need density-based grouping

---

### Exporting Data

#### Export Complete Report

1. **Navigate to Export**:
   - Click "Reports" → "Export System Report"

2. **Generate Report**:
   - Click "Generate Complete Report"
   - Wait for processing (may take time for large datasets)

3. **Download File**:
   - Excel file (.xlsx) downloaded automatically
   - File name: `CVAnalyzer_Report_[DateTime].xlsx`

4. **Report Contents**:
   - **Sheet 1 - Students**: All student data with skills
   - **Sheet 2 - Clusters**: All clusters with members
   - Formatted headers and auto-sized columns

#### Export Student List

1. **From Student List Page**:
   - Click "Export to Excel" button

2. **Download File**:
   - Excel file with all students
   - Columns: ID, Name, Email, Phone, Skills, Experience Count
   - Skills shown as comma-separated list

#### Export Specific Cluster

1. **From Cluster Details Page**:
   - Navigate to specific cluster
   - Click "Export to Excel" button

2. **Download File**:
   - Excel file with cluster details
   - Member list with similarity scores
   - Matching skills highlighted

#### Export Skills Report

1. **Navigate to Reports**:
   - Click "Reports" → "Skills Distribution"

2. **Generate Report**:
   - Click "Export Skills Report"
   - Download Excel file

3. **Report Contents**:
   - Each skill with student count
   - List of students per skill
   - Sorted by popularity (most common skills first)

---

### Finding Similar Students

1. **From Student Details Page**:
   - View any student's details
   - Click "Find Similar Students" button

2. **View Results**:
   - List of students with similar skill profiles
   - Jaccard similarity scores
   - Matching skills highlighted
   - Sorted by similarity (highest first)

3. **Use Results**:
   - Team formation
   - Peer mentoring matches
   - Study group creation
   - Project team building

---

## Administrative Features

### User Management

#### View All Users

1. **Navigate to Users**:
   - Click "Admin" → "Manage Users" (Admin only)

2. **User List Shows**:
   - Email address
   - Full name
   - Roles assigned
   - Account status (Active/Inactive)
   - Last login date

#### Edit User Roles

1. **From User List**:
   - Click "Edit" next to user

2. **Modify Roles**:
   - Check/uncheck role checkboxes:
     - Admin
     - Faculty
     - Staff
   - Users can have multiple roles

3. **Save Changes**:
   - Click "Update User"
   - Changes apply immediately
   - User must re-login for role changes to take effect

#### Reset User Password

1. **From Edit User Page**:
   - Click "Reset Password"

2. **Set New Password**:
   - Enter temporary password
   - Meets password requirements
   - User should change on next login

3. **Notify User**:
   - System does not send email (manual notification required)
   - Provide temporary password securely

#### Deactivate User Account

1. **From Edit User Page**:
   - Uncheck "Is Active" checkbox
   - Save changes

2. **Effect**:
   - User cannot login
   - Existing sessions remain valid until expiry
   - User data and audit logs preserved

---

### System Configuration

#### File Upload Settings

Edit `appsettings.json`:
```json
"FileUpload": {
  "MaxFileSize": 10485760,  // Bytes (10MB default)
  "AllowedExtensions": [".pdf", ".docx"],
  "UploadPath": "uploads/cvs"  // Relative to wwwroot
}
```

#### Clustering Configuration

```json
"Clusters": {
  "AutoRecomputeOnUpload": false,  // Auto-cluster after CV uploads
  "DefaultAlgorithm": "kmeans",    // Default: "kmeans", "dbscan", or "simple"
  "DefaultNumber": 3,              // Default K for K-Means
  "BackgroundIntervalMinutes": 60, // How often to run auto-clustering
  "AutoClusterCooldownHours": 24   // Minimum hours between auto-clusters
}
```

#### JWT Settings

```json
"JwtSettings": {
  "SecretKey": "CHANGE_THIS_IN_PRODUCTION_32_CHARS_MINIMUM",
  "Issuer": "CVAnalyzerSystem",
  "Audience": "CVAnalyzerUsers",
  "ExpiryMinutes": 60
}
```

**Security**: Always change SecretKey in production to a strong random value (32+ characters).

---

### Database Management

#### Apply Migrations

When updating to a new version:
```bash
cd CVAnalyzer.Web
dotnet ef database update
```

#### Create New Migration

After modifying entities:
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

#### Backup Database

**SQL Server**:
```sql
BACKUP DATABASE CVAnalyzerDb TO DISK = 'C:\Backups\CVAnalyzerDb.bak'
```

**LocalDB**:
- Backup file located at: `%USERPROFILE%\`
- Copy `.mdf` and `.ldf` files

---

## Export & Reporting

### Export Service Architecture

The export service generates professional Excel reports using ClosedXML library.

**Location**: `CVAnalyzer.Infrastructure/Services/ExportService.cs`

### Report Types

#### 1. Complete System Report

**Method**: `GenerateReportAsync()`

**Contents**:
- **Students Sheet**:
  - Columns: Student ID, Name, Email, Phone, Skills, Experience Count
  - Skills shown as comma-separated list
  - Sorted by Student ID
  - Blue header row with white text
  - Auto-sized columns

- **Clusters Sheet**:
  - Columns: Cluster Name, Algorithm, Member Count, Common Skills, Created Date
  - Shows all clusters with statistics
  - Common skills extracted from members
  - Formatted date display

**Use Cases**:
- Complete system backup
- Management reporting
- External analysis
- Compliance documentation

#### 2. Student Export

**Method**: `ExportStudentsToExcelAsync(searchTerm?)`

**Features**:
- Optional search term filtering
- All student details
- Skills aggregated per student
- Experience count summary

**Use Cases**:
- Student directory
- Mail merge data
- External system integration

#### 3. Cluster Export

**Method**: `ExportClusterToExcelAsync(clusterId)`

**Contents**:
- Cluster metadata (name, algorithm, description)
- Member details table
- Columns: Student ID, Name, Email, Similarity Score, Matching Skills
- Similarity scores formatted as percentages
- Matching skills as comma-separated list

**Use Cases**:
- Team formation documentation
- Cluster analysis
- External review

#### 4. Skills Distribution Report

**Method**: `ExportSkillsReportAsync()`

**Contents**:
- All unique skills in system
- Student count per skill
- Complete student list per skill
- Sorted by popularity (descending)
- Yellow header for emphasis

**Use Cases**:
- Skills gap analysis
- Curriculum planning
- Industry trend analysis

### Export File Naming

All exports use timestamped filenames:
- Format: `[Type]_[DateTime].xlsx`
- Example: `Students_20251116_143022.xlsx`
- Prevents overwrite conflicts
- Enables version tracking

---

## Machine Learning Features

### ML.NET Integration

CVAnalyzer uses **Microsoft.ML** (ML.NET) for advanced clustering and similarity analysis.

**Library Version**: 4.0.2
**Location**: `CVAnalyzer.Infrastructure/Services/MLClusteringService.cs`

### Feature Engineering

#### Skill Vectorization

**Process**:
1. Extract all unique skills across all students
2. Create fixed-size binary vector for each student
3. Vector[i] = 1 if student has skill[i], else 0
4. Normalize skill names for consistency

**Example**:
```
Skills Universe: [C#, Python, JavaScript, SQL, React]

Student A (C#, Python, SQL):
Vector: [1, 1, 0, 1, 0]

Student B (JavaScript, React):
Vector: [0, 0, 1, 0, 1]

Student C (C#, JavaScript, React):
Vector: [1, 0, 1, 0, 1]
```

### K-Means Algorithm

**Implementation Details**:
- **Algorithm**: K-Means++ initialization
- **Distance Metric**: Euclidean distance
- **Convergence**: Maximum iterations (100) or convergence threshold
- **Feature Count**: Dynamic (based on unique skills in dataset)

**Training Pipeline**:
```csharp
var pipeline = mlContext.Transforms
    .Concatenate("Features", nameof(StudentClusterData.Features))
    .Append(mlContext.Clustering.Trainers.KMeans(
        featureColumnName: "Features",
        numberOfClusters: k
    ));
```

**Quality Metrics**:
- **Silhouette Score**: Measures cluster cohesion and separation
  - Range: -1 to +1
  - > 0.7: Strong clustering
  - 0.5-0.7: Moderate clustering
  - < 0.5: Weak clustering

**Advantages**:
- Fast and scalable
- Deterministic results
- Works well with large datasets
- Easy to interpret

**Limitations**:
- Requires predefined K
- Assumes spherical clusters
- Sensitive to outliers

### DBSCAN Algorithm

**Implementation Details**:
- **Algorithm**: Custom DBSCAN implementation
- **Distance Metric**: Euclidean distance in skill space
- **Parameters**:
  - `epsilon` (ε): Neighborhood radius
  - `minPoints`: Minimum points for core point

**Classification**:
- **Core Point**: Has ≥ minPoints within epsilon radius
- **Border Point**: Within epsilon of core point but < minPoints neighbors
- **Noise Point**: Neither core nor border (cluster -1)

**Clustering Process**:
```
1. Mark all points as unvisited
2. For each unvisited point:
   a. Mark as visited
   b. Find neighbors within epsilon
   c. If neighbors < minPoints: mark as noise
   d. Else: create new cluster
   e. Expand cluster from neighbors
3. Return clusters + noise points
```

**Advantages**:
- Automatic cluster count detection
- Identifies outliers
- Handles arbitrary cluster shapes
- No spherical assumption

**Limitations**:
- Sensitive to epsilon and minPoints parameters
- Slower than K-Means for large datasets
- Difficult to tune for varying density

**Parameter Tuning Guidelines**:
- **Epsilon**:
  - Too small: Many noise points, many small clusters
  - Too large: Few large clusters, few noise points
  - Typical: 0.3-0.7 for normalized features

- **MinPoints**:
  - Too small: Sensitive to noise
  - Too large: Merges distinct clusters
  - Typical: 2-5 for small datasets, 5-10 for large

### Semantic Skill Matching

**Synonym Dictionary**:
Hardcoded mappings for 50+ common skills:

```csharp
{ "JavaScript", ["JS", "ECMAScript", "javascript", "es6"] },
{ "Python", ["python", "py", "python3"] },
{ "C#", ["CSharp", "C Sharp", "csharp", "c-sharp"] },
{ "AWS", ["Amazon Web Services", "aws", "amazon-web-services"] },
{ "React", ["ReactJS", "React.js", "react-js"] }
// ... and many more
```

**Similarity Calculation**:

1. **Exact Match** (1.0):
   ```csharp
   if (normalizedSkill1 == normalizedSkill2) return 1.0;
   ```

2. **Synonym Match** (0.9):
   ```csharp
   if (synonyms[skill1].Contains(skill2)) return 0.9;
   ```

3. **Partial Match** (0.7):
   ```csharp
   if (skill1.Contains(skill2) || skill2.Contains(skill1)) return 0.7;
   ```

4. **Levenshtein Distance** (0.0-1.0):
   ```csharp
   int distance = CalculateLevenshteinDistance(skill1, skill2);
   int maxLength = Math.Max(skill1.Length, skill2.Length);
   return 1.0 - ((double)distance / maxLength);
   ```

**Levenshtein Distance**:
- Minimum number of single-character edits (insertions, deletions, substitutions)
- Example: "JavaScript" → "JavaScrpt" = distance 1
- Normalized by string length for 0-1 similarity

**Use Cases**:
- Skill matching across different CV formats
- Handling typos and variations
- Recommending related skills
- Improving search accuracy

---

## API Reference

### Authentication Endpoints

#### POST /api/auth/login
Authenticate user and receive JWT token.

**Request**:
```json
{
  "email": "user@example.com",
  "password": "Password@123"
}
```

**Response** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-11-16T15:30:00Z",
  "email": "user@example.com",
  "roles": ["Faculty"]
}
```

**Response** (401 Unauthorized):
```json
{
  "message": "Invalid email or password"
}
```

---

### Student Endpoints

#### GET /Student/GetAll
Retrieve all students (JSON).

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "studentId": "STU001",
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "123-456-7890",
    "skillCount": 5,
    "cvCount": 2
  }
]
```

#### GET /Student/GetById/{id}
Get specific student details.

**Response** (200 OK):
```json
{
  "id": 1,
  "studentId": "STU001",
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "123-456-7890",
  "skills": ["C#", "Python", "SQL"],
  "experiences": [
    {
      "company": "Tech Corp",
      "position": "Developer",
      "duration": "2020-2022"
    }
  ],
  "cvDocuments": [
    {
      "fileName": "resume.pdf",
      "uploadDate": "2025-01-15T10:00:00Z",
      "fileSize": 245760
    }
  ]
}
```

---

### Clustering API Endpoints

#### POST /api/ClusterJobs/kmeans
Enqueue K-Means clustering job.

**Headers**:
```
Authorization: Bearer {token}
```

**Query Parameters**:
- `k` (int, required): Number of clusters

**Request**:
```
POST /api/ClusterJobs/kmeans?k=3
```

**Response** (202 Accepted):
```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "Pending",
  "message": "K-Means clustering job queued successfully"
}
```

#### POST /api/ClusterJobs/dbscan
Enqueue DBSCAN clustering job.

**Query Parameters**:
- `eps` (double, required): Epsilon parameter
- `minPts` (int, required): Minimum points parameter

**Request**:
```
POST /api/ClusterJobs/dbscan?eps=0.5&minPts=2
```

**Response** (202 Accepted):
```json
{
  "jobId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "status": "Pending",
  "message": "DBSCAN clustering job queued successfully"
}
```

#### GET /api/ClusterJobs/{jobId}
Check clustering job status.

**Response** (200 OK - Pending):
```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "Pending",
  "startedAt": "2025-11-16T14:00:00Z"
}
```

**Response** (200 OK - Running):
```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "Running",
  "startedAt": "2025-11-16T14:00:00Z"
}
```

**Response** (200 OK - Completed):
```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "Completed",
  "startedAt": "2025-11-16T14:00:00Z",
  "completedAt": "2025-11-16T14:02:30Z",
  "clusterId": 15,
  "message": "K-Means clustering completed successfully with 3 clusters"
}
```

**Response** (200 OK - Failed):
```json
{
  "jobId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "Failed",
  "startedAt": "2025-11-16T14:00:00Z",
  "completedAt": "2025-11-16T14:01:00Z",
  "errorMessage": "Insufficient students for clustering (minimum 10 required)"
}
```

---

### Export Endpoints

#### GET /Export/Download
Download complete system report.

**Query Parameters**:
- `type` (string, optional): Report type
  - `students`: Student export only
  - `clusters`: Cluster export only
  - `skills`: Skills distribution
  - (empty): Complete report

**Response**:
- Content-Type: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Content-Disposition: `attachment; filename="Report_[timestamp].xlsx"`
- Binary Excel file

---

## Configuration Options

### appsettings.json Complete Reference

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CVAnalyzerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },

  "JwtSettings": {
    "SecretKey": "DevSecretKey_ChangeInProduction_32CharactersMinimum!@#",
    "Issuer": "CVAnalyzerSystem",
    "Audience": "CVAnalyzerUsers",
    "ExpiryMinutes": 60
  },

  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".pdf", ".docx"],
    "UploadPath": "uploads/cvs"
  },

  "Clusters": {
    "AutoRecomputeOnUpload": false,
    "DefaultAlgorithm": "kmeans",
    "DefaultNumber": 3,
    "BackgroundIntervalMinutes": 60,
    "AutoClusterCooldownHours": 24
  },

  "Storage": {
    "Provider": "Local",
    "AzureBlobConnectionString": "",
    "AzureBlobContainer": "cvdocuments"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },

  "AllowedHosts": "*"
}
```

### Configuration Explanations

#### ConnectionStrings

**DefaultConnection**: Database connection string
- **LocalDB**: `Server=(localdb)\\mssqllocaldb;...`
- **SQL Server**: `Server=SERVER_NAME;Database=CVAnalyzerDb;User Id=USERNAME;Password=PASSWORD;TrustServerCertificate=true`
- **Azure SQL**: `Server=tcp:SERVER.database.windows.net,1433;Database=CVAnalyzerDb;User Id=USERNAME;Password=PASSWORD;Encrypt=True;`

#### JwtSettings

**SecretKey**:
- Must be 32+ characters
- Use strong random value in production
- Never commit production key to source control

**Issuer**:
- JWT token issuer claim
- Should match your application domain

**Audience**:
- JWT token audience claim
- Should match your application users

**ExpiryMinutes**:
- Token lifetime in minutes
- Balance security vs. user experience
- Typical: 30-120 minutes

#### FileUpload

**MaxFileSize**:
- In bytes (10485760 = 10MB)
- Consider server memory and network bandwidth
- Larger files take longer to process

**AllowedExtensions**:
- File type whitelist
- Security measure against malicious uploads
- Current: PDF and DOCX only

**UploadPath**:
- Relative to wwwroot
- Files stored at: `wwwroot/uploads/cvs/[StudentId]/[Guid].ext`

#### Clusters

**AutoRecomputeOnUpload**:
- `true`: Automatically re-cluster after bulk uploads
- `false`: Manual clustering only
- Recommendation: false (prevents performance issues)

**DefaultAlgorithm**:
- Options: `"simple"`, `"kmeans"`, `"dbscan"`
- Used for auto-clustering
- Recommendation: `"kmeans"` for balanced results

**DefaultNumber**:
- Default K for K-Means
- Typical: 3-5 clusters
- Ignored for DBSCAN

**BackgroundIntervalMinutes**:
- How often background service checks for clustering tasks
- Recommendation: 60 (1 hour)

**AutoClusterCooldownHours**:
- Minimum time between automatic clusterings
- Prevents excessive re-computation
- Recommendation: 24 hours

#### Storage

**Provider**:
- Options: `"Local"`, `"AzureBlob"`
- Current: Local file system
- Future: Azure Blob Storage support

**AzureBlobConnectionString**:
- Connection string for Azure Storage Account
- Required if Provider = "AzureBlob"

**AzureBlobContainer**:
- Blob container name for CV storage
- Required if Provider = "AzureBlob"

---

## Technical Details

### Technology Stack

#### Backend
- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2019+ (or LocalDB for development)
- **Authentication**: ASP.NET Core Identity 8.0
- **API Security**: JWT Bearer Authentication

#### Libraries & Packages

**Data Access**:
- Microsoft.EntityFrameworkCore 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- Microsoft.EntityFrameworkCore.Tools 8.0.0

**Authentication & Security**:
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- System.IdentityModel.Tokens.Jwt 7.0.0

**Document Processing**:
- iText7 8.0.2 (PDF parsing)
- DocumentFormat.OpenXml 2.20.0 (DOCX parsing)

**Excel Export**:
- ClosedXML 0.102.3 (Excel generation)
- EPPlus 7.0.0 (Alternative Excel library)

**Machine Learning**:
- Microsoft.ML 4.0.2 (ML.NET)

**Validation**:
- FluentValidation 11.9.0

**Utilities**:
- Newtonsoft.Json 13.0.3

#### Frontend
- **Framework**: ASP.NET Core MVC with Razor Views
- **CSS**: Bootstrap 5.1
- **JavaScript**: Vanilla JS with jQuery 3.6
- **Icons**: Bootstrap Icons

### Database Schema

#### Core Tables

**Students**:
```sql
CREATE TABLE Students (
    Id INT PRIMARY KEY IDENTITY,
    StudentId NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20),
    CreatedDate DATETIME2 NOT NULL,
    ModifiedDate DATETIME2,
    UploadedByUserId NVARCHAR(450),
    CONSTRAINT FK_Students_Users FOREIGN KEY (UploadedByUserId)
        REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_Students_StudentId ON Students(StudentId);
CREATE INDEX IX_Students_Email ON Students(Email);
```

**Skills**:
```sql
CREATE TABLE Skills (
    Id INT PRIMARY KEY IDENTITY,
    SkillName NVARCHAR(100) NOT NULL,
    NormalizedName NVARCHAR(100) NOT NULL UNIQUE,
    Category NVARCHAR(50),
    CreatedDate DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_Skills_NormalizedName ON Skills(NormalizedName);
CREATE INDEX IX_Skills_Category ON Skills(Category);
```

**StudentSkills**:
```sql
CREATE TABLE StudentSkills (
    Id INT PRIMARY KEY IDENTITY,
    StudentId INT NOT NULL,
    SkillId INT NOT NULL,
    ProficiencyLevel NVARCHAR(50),
    ExtractedText NVARCHAR(500),
    ConfidenceScore FLOAT,
    CONSTRAINT FK_StudentSkills_Students FOREIGN KEY (StudentId)
        REFERENCES Students(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StudentSkills_Skills FOREIGN KEY (SkillId)
        REFERENCES Skills(Id) ON DELETE CASCADE
);

CREATE INDEX IX_StudentSkills_StudentId ON StudentSkills(StudentId);
CREATE INDEX IX_StudentSkills_SkillId ON StudentSkills(SkillId);
CREATE INDEX IX_StudentSkills_StudentId_SkillId ON StudentSkills(StudentId, SkillId);
```

**CVDocuments**:
```sql
CREATE TABLE CVDocuments (
    Id INT PRIMARY KEY IDENTITY,
    StudentId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    BlobPath NVARCHAR(500),
    FileSize BIGINT NOT NULL,
    FileType NVARCHAR(20) NOT NULL,
    ProcessingStatus NVARCHAR(50) NOT NULL,
    ErrorMessage NVARCHAR(MAX),
    UploadDate DATETIME2 NOT NULL,
    ProcessedDate DATETIME2,
    CONSTRAINT FK_CVDocuments_Students FOREIGN KEY (StudentId)
        REFERENCES Students(Id) ON DELETE CASCADE
);

CREATE INDEX IX_CVDocuments_StudentId ON CVDocuments(StudentId);
CREATE INDEX IX_CVDocuments_UploadDate ON CVDocuments(UploadDate);
CREATE INDEX IX_CVDocuments_StudentId_UploadDate ON CVDocuments(StudentId, UploadDate);
```

**StudentClusters**:
```sql
CREATE TABLE StudentClusters (
    Id INT PRIMARY KEY IDENTITY,
    ClusterName NVARCHAR(200) NOT NULL,
    Algorithm NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    MemberCount INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL,
    CreatedByUserId NVARCHAR(450),
    CONSTRAINT FK_StudentClusters_Users FOREIGN KEY (CreatedByUserId)
        REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_StudentClusters_CreatedDate ON StudentClusters(CreatedDate);
```

**ClusterMembers**:
```sql
CREATE TABLE ClusterMembers (
    Id INT PRIMARY KEY IDENTITY,
    ClusterId INT NOT NULL,
    StudentId INT NOT NULL,
    SimilarityScore FLOAT,
    MatchingSkills NVARCHAR(MAX),
    CONSTRAINT FK_ClusterMembers_Clusters FOREIGN KEY (ClusterId)
        REFERENCES StudentClusters(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ClusterMembers_Students FOREIGN KEY (StudentId)
        REFERENCES Students(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ClusterMembers_ClusterId_StudentId UNIQUE (ClusterId, StudentId)
);

CREATE UNIQUE INDEX IX_ClusterMembers_ClusterId_StudentId
    ON ClusterMembers(ClusterId, StudentId);
```

**AuditLogs**:
```sql
CREATE TABLE AuditLogs (
    Id INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(450),
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100),
    EntityId NVARCHAR(100),
    Details NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    Timestamp DATETIME2 NOT NULL,
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId)
        REFERENCES AspNetUsers(Id)
);

CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_UserId_Timestamp ON AuditLogs(UserId, Timestamp);
CREATE INDEX IX_AuditLogs_EntityType_EntityId ON AuditLogs(EntityType, EntityId);
```

### Performance Optimizations

#### Database Indexes
- **Clustered Indexes**: Primary keys (Id columns)
- **Non-Clustered Indexes**: Foreign keys, search columns, date columns
- **Composite Indexes**: Common query patterns (StudentId + UploadDate)
- **Unique Indexes**: Enforce data integrity (StudentId, NormalizedName)

#### Query Optimizations
- **Eager Loading**: `.Include()` for related entities
- **Projection**: Select only needed columns
- **Pagination**: Limit results with `.Take()` and `.Skip()`
- **Caching**: In-memory caching for skills list
- **Async Operations**: All database operations are async

#### File Storage
- **Local Storage**: Fast access for single-server deployments
- **Organized Structure**: Files grouped by student ID
- **GUID Filenames**: Prevent naming conflicts
- **Metadata in Database**: Quick queries without file system access

#### Background Processing
- **Queue-Based**: Non-blocking clustering operations
- **Cancellation Tokens**: Graceful shutdown
- **Error Handling**: Retry logic and error logging
- **Cooldown Periods**: Prevent excessive resource usage

### Security Considerations

#### Authentication
- **Identity Framework**: Industry-standard authentication
- **Password Hashing**: PBKDF2 with random salt
- **Account Lockout**: Protection against brute force
- **JWT Tokens**: Stateless API authentication

#### Authorization
- **Role-Based**: Admin, Faculty, Staff roles
- **Policy-Based**: Flexible authorization policies
- **Controller/Action Level**: Granular access control
- **Resource-Based**: Ownership checks for sensitive operations

#### Input Validation
- **Model Validation**: Data annotations and FluentValidation
- **File Type Validation**: Whitelist approach
- **File Size Limits**: Prevent DoS via large uploads
- **SQL Injection Prevention**: Parameterized queries via EF Core

#### Data Protection
- **HTTPS**: All communication encrypted
- **HttpOnly Cookies**: Protection against XSS
- **CORS Configuration**: Controlled cross-origin access
- **Rate Limiting**: 100 requests per minute per user

#### File Upload Security
- **Extension Whitelist**: Only .pdf and .docx allowed
- **Size Limits**: 10MB maximum
- **Virus Scanning**: (Recommended for production)
- **Storage Isolation**: Files stored outside webroot execution path

### Sample Data

The project includes diverse sample CVs for testing:

**Location**: `/home/user/CVAnalyzerI/samples/cvs/`

**Standard Samples**:
- `sample_cv_john_doe.pdf`: Software developer profile
- `sample_cv_jane_smith.docx`: Data analyst profile
- `sample_cv_alex_khan.txt`: Full-stack developer

**Edge Case Testing**:
- `sample_cv_student_multiple_skills.pdf`: 15+ skills
- `sample_cv_edge_cases.md`: Missing fields, special characters
- `sample_cv_long_text.docx`: Performance testing (5000+ words)

**Multilingual Support**:
- `sample_cv_chinese_zh.pdf`: Chinese language CV
- `sample_cv_maria_garcia_es.pdf`: Spanish language CV

**Use Cases**:
- Testing CV parsing accuracy
- Validating clustering algorithms
- Performance benchmarking
- Internationalization testing

---

## Troubleshooting

### Common Issues

#### Database Connection Errors
**Symptom**: "Cannot connect to database" error on startup

**Solutions**:
1. Check SQL Server service is running
2. Verify connection string in appsettings.json
3. For LocalDB: Run `sqllocaldb start mssqllocaldb`
4. Check firewall settings for SQL Server port (1433)
5. Verify user permissions on database

#### CV Upload Failures
**Symptom**: CV upload fails with "Processing failed" message

**Solutions**:
1. Check file format (must be PDF or DOCX)
2. Verify file size is under 10MB
3. Ensure CV contains readable text (not scanned image)
4. Check upload directory permissions
5. Review error logs for specific parsing errors

#### Clustering Fails
**Symptom**: "Clustering failed" or no clusters created

**Solutions**:
1. Ensure minimum 10 students in database
2. Check students have skills extracted
3. For K-Means: K must be less than student count
4. For DBSCAN: Adjust epsilon and minPoints parameters
5. Review background job logs

#### Login Issues
**Symptom**: Cannot login with valid credentials

**Solutions**:
1. Check account is not locked (wait 15 minutes)
2. Verify password meets requirements
3. Clear browser cookies and cache
4. Check database connection
5. Verify user exists in AspNetUsers table

#### Export Fails
**Symptom**: Export button doesn't download file

**Solutions**:
1. Check for JavaScript errors in browser console
2. Verify sufficient data exists for export
3. Check server disk space
4. Review application logs for errors
5. Try different browser

### Logging

**Log Locations**:
- **Development**: Console output
- **Production**: Configure file logging or external service

**Log Levels**:
- **Error**: Critical issues requiring attention
- **Warning**: Potential issues
- **Information**: General flow of application
- **Debug**: Detailed diagnostic information

**Enable Detailed EF Logging**:
```json
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore": "Debug"
  }
}
```

---

## Best Practices

### For Administrators

1. **Regular Backups**:
   - Schedule daily database backups
   - Test restore procedures regularly
   - Keep backups offsite or in cloud storage

2. **Security**:
   - Change default admin password immediately
   - Use strong JWT secret in production
   - Enable HTTPS in production
   - Regular security audits of user accounts

3. **Performance Monitoring**:
   - Monitor database size and growth
   - Review slow query logs
   - Check disk space for file uploads
   - Monitor background job queue length

4. **Maintenance**:
   - Regular cleanup of old audit logs
   - Archive old CV documents
   - Review and optimize database indexes
   - Update NuGet packages regularly

### For Faculty/Users

1. **CV Uploads**:
   - Use descriptive student IDs
   - Ensure CVs contain clear text (not images)
   - Include student ID in CV document
   - Use consistent naming conventions

2. **Clustering**:
   - Start with simple clustering to understand data
   - Use K-Means for balanced groups
   - Use DBSCAN to find natural groupings
   - Experiment with parameters for best results

3. **Searching**:
   - Use specific search terms
   - Filter by skills for targeted results
   - Export filtered results for external analysis

### For Developers

1. **Code Organization**:
   - Follow Clean Architecture principles
   - Keep controllers thin, services fat
   - Use dependency injection
   - Async/await for all I/O operations

2. **Database**:
   - Always use migrations for schema changes
   - Add indexes for frequently queried columns
   - Use eager loading to prevent N+1 queries
   - Test with realistic data volumes

3. **Error Handling**:
   - Use try-catch in controllers
   - Log errors with context
   - Return appropriate HTTP status codes
   - Provide user-friendly error messages

4. **Testing**:
   - Unit test service layer
   - Integration test repositories
   - Use sample data for consistent tests
   - Test edge cases and error conditions

---

## Future Enhancements

### Planned Features

1. **Advanced Analytics**:
   - Skill trend analysis over time
   - Predictive modeling for student success
   - Industry alignment reports
   - Recommendation engine for skill development

2. **Enhanced ML**:
   - Deep learning for CV parsing (OCR for images)
   - Neural networks for similarity matching
   - Automatic skill categorization
   - Sentiment analysis on work descriptions

3. **Integration**:
   - Azure Blob Storage for file storage
   - Email notifications for job completion
   - REST API expansion
   - Integration with learning management systems

4. **User Experience**:
   - Real-time clustering progress
   - Drag-and-drop CV upload
   - Advanced filtering and sorting
   - Dashboard visualizations with charts

5. **Scalability**:
   - Distributed caching with Redis
   - Message queue for background jobs (RabbitMQ)
   - Microservices architecture
   - Container support (Docker)

---

## Support & Contribution

### Getting Help

1. **Documentation**: Review this guide thoroughly
2. **Code Comments**: Check inline code documentation
3. **Sample Data**: Use provided samples for testing
4. **Logs**: Review application logs for errors

### Reporting Issues

When reporting issues, include:
- Steps to reproduce
- Expected vs. actual behavior
- Screenshots if applicable
- Relevant log entries
- Environment details (OS, .NET version, database)

### Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/YourFeature`
3. Follow existing code style and architecture
4. Add unit tests for new features
5. Update documentation
6. Submit pull request with detailed description

---

## Appendix

### File Structure Reference

```
CVAnalyzerI/
├── CVAnalyzer.Core/
│   ├── Entities/
│   │   ├── AuditLog.cs
│   │   ├── ClusterMember.cs
│   │   ├── CVDocument.cs
│   │   ├── Experience.cs
│   │   ├── Skill.cs
│   │   ├── Student.cs
│   │   ├── StudentCluster.cs
│   │   ├── StudentSkill.cs
│   │   └── ApplicationUser.cs
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IStudentRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Enums/
│       ├── UserRoles.cs
│       └── Permissions.cs
│
├── CVAnalyzer.Application/
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Student/
│   │   ├── CV/
│   │   ├── Cluster/
│   │   └── User/
│   ├── Services/
│   │   ├── ICVParserService.cs
│   │   ├── ICVProcessingService.cs
│   │   ├── IClusteringService.cs
│   │   ├── IMLClusteringService.cs
│   │   ├── IStudentService.cs
│   │   ├── IExportService.cs
│   │   ├── IFileStorageService.cs
│   │   ├── IAuthService.cs
│   │   ├── IAuditService.cs
│   │   └── ClusteringJobStatus.cs
│   └── Models/
│
├── CVAnalyzer.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── UnitOfWork.cs
│   ├── Migrations/
│   ├── Repositories/
│   │   ├── Repository.cs
│   │   └── StudentRepository.cs
│   ├── Services/
│   │   ├── CVParserService.cs
│   │   ├── CVProcessingService.cs
│   │   ├── ClusteringService.cs
│   │   ├── MLClusteringService.cs
│   │   ├── StudentService.cs
│   │   ├── ExportService.cs
│   │   ├── FileStorageService.cs
│   │   ├── AuthService.cs
│   │   ├── UserManagementService.cs
│   │   ├── AuditService.cs
│   │   ├── SkillCacheService.cs
│   │   ├── ClusteringBackgroundService.cs
│   │   ├── ClusteringJobService.cs
│   │   └── QueuedHostedService.cs
│   └── Queue/
│       ├── IBackgroundTaskQueue.cs
│       └── BackgroundTaskQueue.cs
│
├── CVAnalyzer.Web/
│   ├── Controllers/
│   │   ├── AccountController.cs
│   │   ├── CVController.cs
│   │   ├── StudentController.cs
│   │   ├── ClusterController.cs
│   │   ├── ExportController.cs
│   │   ├── ReportController.cs
│   │   ├── UserController.cs
│   │   ├── HomeController.cs
│   │   ├── DashboardController.cs
│   │   └── ClusterJobsController.cs
│   ├── Views/
│   │   ├── Account/
│   │   ├── CV/
│   │   ├── Student/
│   │   ├── Cluster/
│   │   ├── User/
│   │   ├── Home/
│   │   └── Shared/
│   ├── Pages/
│   │   └── Export/
│   │       ├── Index.cshtml
│   │       └── Index.cshtml.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   ├── lib/
│   │   └── uploads/
│   │       └── cvs/
│   ├── Program.cs
│   └── appsettings.json
│
├── samples/
│   └── cvs/
│       ├── sample_cv_john_doe.pdf
│       ├── sample_cv_jane_smith.docx
│       ├── sample_cv_alex_khan.txt
│       ├── sample_cv_student_multiple_skills.pdf
│       ├── sample_cv_chinese_zh.pdf
│       ├── sample_cv_maria_garcia_es.pdf
│       ├── sample_cv_edge_cases.md
│       └── sample_cv_long_text.docx
│
├── .gitignore
├── README.md
├── FEATURES_GUIDE.md (this file)
└── StudentCVAnalyzer.sln
```

### Glossary

- **Clean Architecture**: Software design pattern separating concerns into layers
- **DTO**: Data Transfer Object - object for transferring data between layers
- **DBSCAN**: Density-Based Spatial Clustering of Applications with Noise
- **Eager Loading**: Loading related entities in a single query
- **Jaccard Similarity**: Measure of similarity between two sets
- **JWT**: JSON Web Token - compact token format for authentication
- **K-Means**: Clustering algorithm that partitions data into K clusters
- **Levenshtein Distance**: Minimum edits needed to change one string to another
- **ML.NET**: Microsoft's machine learning framework for .NET
- **N+1 Query Problem**: Performance issue from loading related entities separately
- **Silhouette Score**: Metric for cluster quality (-1 to +1)
- **Unit of Work**: Pattern for managing transactions across repositories

---

## License

*Add your license information here*

## Version History

- **Version 1.0** (2025-11-16): Initial comprehensive documentation
  - All core features documented
  - Complete API reference
  - User guide sections
  - Technical details included

---

**Document Last Updated**: November 16, 2025
**CVAnalyzer Version**: 1.0
**Framework**: ASP.NET Core 8.0
