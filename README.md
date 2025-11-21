# Contract Monthly Claim System (CMCS) - Part 3

## Project Overview
The Contract Monthly Claim System is a comprehensive web application built with ASP.NET Core that allows lecturers to submit monthly claims for hours worked, and enables Programme Coordinators and Academic Managers to review and approve these claims. HR personnel can manage all users and generate reports/invoices.

## Technology Stack
- **Framework**: ASP.NET Core 8.0 (Razor Pages)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: Session-based authentication
- **PDF Generation**: For invoices and reports
- **Frontend**: Bootstrap 5, jQuery, custom JavaScript

---

## Changes from Part 2 to Part 3

### Major Updates Based on Lecturer Feedback

#### 1. **HR Super User Role Implementation**
- **Previous**: No dedicated HR role or user management system
- **Current**: 
  - HR role added as a "super user" with full system access
  - HR creates and manages all user accounts (Lecturers, Coordinators, Managers)
  - HR can update user information including hourly rates
  - HR generates reports and invoices using LINQ queries
  - **No registration page** - HR provides login credentials to users

#### 2. **Removed Monthly Claim Limit**
- **Previous**: Lecturers could only submit 1 claim per month
- **Current**: Lecturers can submit multiple claims per month
  - Useful for tracking different projects, courses, or work periods
  - Each claim is tracked individually through the approval process
  - Validation ensures total monthly hours don't exceed reasonable limits (180 hours)

#### 3. **Enhanced Lecturer Experience**
- **Previous**: Lecturers manually entered hourly rates
- **Current**:
  - Lecturer name, surname, and hourly rate automatically pulled from database
  - Auto-calculation: Enter hours worked → System calculates total amount
  - Real-time validation for hours worked (max 180 hours per month check)
  - Improved claim tracking with detailed status updates

#### 4. **Entity Framework Core with Database**
- **Previous**: JSON file storage
- **Current**:
  - Full EF Core implementation with SQL Server
  - Database-first approach (no migrations required)
  - Proper relational data structure
  - Improved data integrity and performance

#### 5. **Enhanced Security & Access Control**
- **Session Management**: Implemented across all controllers
- **Role-based Access Control**: Users cannot access pages they're not authorized for
- **Custom Login**: Secure authentication without ASP.NET Identity (custom implementation)
- **Page Guards**: Middleware and filters prevent unauthorized access

#### 6. **Report & Invoice Generation**
- **LINQ Queries**: Generate comprehensive reports
- **PDF Export**: Download invoices and reports as PDFs
- **HR Dashboard**: View all claims, filter by status, user, date range

---

## Key Features & Implementation Details

### 1. Database Models

#### User Model
```csharp
public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } // Hashed
    public string Role { get; set; } // HR, Lecturer, Coordinator, Manager
    public decimal HourlyRate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Claim> Claims { get; set; }
}
```

#### Claim Model
```csharp
public class Claim
{
    public int ClaimId { get; set; }
    public int LecturerId { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ClaimDate { get; set; }
    public string Status { get; set; } // Pending, CoordinatorApproved, ManagerApproved, Rejected
    public string? RejectionReason { get; set; }
    public string? DocumentPath { get; set; }
    public DateTime SubmittedDate { get; set; }
    public int? ReviewedByCoordinatorId { get; set; }
    public int? ReviewedByManagerId { get; set; }
    public DateTime? CoordinatorReviewDate { get; set; }
    public DateTime? ManagerReviewDate { get; set; }
    
    public User Lecturer { get; set; }
    public User? ReviewedByCoordinator { get; set; }
    public User? ReviewedByManager { get; set; }
}
```

### 2. Session Management Implementation

**Session Helper** (`SessionHelper.cs`):
```csharp
public static class SessionHelper
{
    public const string UserIdKey = "UserId";
    public const string UserRoleKey = "UserRole";
    public const string UserNameKey = "UserName";

    public static void SetUserSession(ISession session, User user)
    {
        session.SetInt32(UserIdKey, user.UserId);
        session.SetString(UserRoleKey, user.Role);
        session.SetString(UserNameKey, $"{user.FirstName} {user.LastName}");
    }

    public static bool IsLoggedIn(ISession session)
    {
        return session.GetInt32(UserIdKey).HasValue;
    }

    public static string GetUserRole(ISession session)
    {
        return session.GetString(UserRoleKey);
    }
}
```

### 3. Validation Service

**Hours Validation**:
```csharp
public class ClaimValidationService
{
    public ValidationResult ValidateClaimSubmission(Claim claim, List<Claim> existingClaims)
    {
        // Check if hours exceed 180 for the month
        var monthStart = new DateTime(claim.ClaimDate.Year, claim.ClaimDate.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        
        var totalHoursThisMonth = existingClaims
            .Where(c => c.LecturerId == claim.LecturerId 
                     && c.ClaimDate >= monthStart 
                     && c.ClaimDate <= monthEnd
                     && c.Status != "Rejected")
            .Sum(c => c.HoursWorked);
        
        if (totalHoursThisMonth + claim.HoursWorked > 180)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                Message = $"Total hours for this month ({totalHoursThisMonth + claim.HoursWorked}) exceeds 180 hours limit." 
            };
        }
        
        return new ValidationResult { IsValid = true };
    }
}
```

### 4. Report Generation with LINQ

**Invoice Generation Service**:
```csharp
public class InvoiceGenerationService
{
    public Invoice GenerateInvoice(int lecturerId, DateTime startDate, DateTime endDate)
    {
        var claims = _context.Claims
            .Include(c => c.Lecturer)
            .Where(c => c.LecturerId == lecturerId 
                     && c.Status == "ManagerApproved"
                     && c.ClaimDate >= startDate 
                     && c.ClaimDate <= endDate)
            .OrderBy(c => c.ClaimDate)
            .ToList();
        
        return new Invoice
        {
            LecturerName = $"{claims.First().Lecturer.FirstName} {claims.First().Lecturer.LastName}",
            StartDate = startDate,
            EndDate = endDate,
            TotalHours = claims.Sum(c => c.HoursWorked),
            TotalAmount = claims.Sum(c => c.TotalAmount),
            Claims = claims
        };
    }
    
    public byte[] GeneratePdfInvoice(Invoice invoice)
    {
        // PDF generation logic using QuestPDF or similar library
    }
}
```

### 5. Authorization Filters

**Custom Authorization Attribute**:
```csharp
public class AuthorizeRoleAttribute : Attribute, IPageFilter
{
    private readonly string[] _allowedRoles;
    
    public AuthorizeRoleAttribute(params string[] roles)
    {
        _allowedRoles = roles;
    }
    
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        var session = context.HttpContext.Session;
        
        if (!SessionHelper.IsLoggedIn(session))
        {
            context.Result = new RedirectToPageResult("/Account/Login");
            return;
        }
        
        var userRole = SessionHelper.GetUserRole(session);
        if (!_allowedRoles.Contains(userRole))
        {
            context.Result = new StatusCodeResult(403); // Forbidden
        }
    }
}
```

---

## User Roles & Permissions

### HR (Super User)
- ✅ Create, update, and manage all user accounts
- ✅ Set hourly rates for lecturers
- ✅ Generate reports and invoices for any lecturer
- ✅ View all claims across the system
- ✅ Export data to PDF
- ✅ View system statistics

### Lecturer
- ✅ Submit multiple claims per month
- ✅ Auto-populated personal information
- ✅ Upload supporting documents
- ✅ Track claim status in real-time
- ✅ View claim history
- ❌ Cannot modify hourly rate
- ❌ Cannot access admin pages

### Programme Coordinator
- ✅ Review pending claims (first approval level)
- ✅ Approve or reject claims with comments
- ✅ View all claims requiring coordinator review
- ✅ Filter and search claims
- ❌ Cannot modify user information

### Academic Manager
- ✅ Review coordinator-approved claims (final approval)
- ✅ Approve or reject claims
- ✅ View all claims requiring manager review
- ✅ Final authority on claim approval
- ❌ Cannot modify user information

---

## Installation & Setup

### Prerequisites
- Visual Studio 2022 (or later)
- .NET 8.0 SDK
- SQL Server (LocalDB or full version)

### Database Setup

1. **Update Connection String** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CMCSDatabase;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

2. **Create Database** (Run in SQL Server Management Studio or Visual Studio):
```sql
CREATE DATABASE CMCSDatabase;
GO

USE CMCSDatabase;
GO

-- Run the complete schema script provided in DatabaseSchema.sql
```

3. **Seed Initial HR User**:
```sql
INSERT INTO Users (FirstName, LastName, Email, Password, Role, HourlyRate, CreatedDate, IsActive)
VALUES ('Admin', 'HR', 'hr@cmcs.com', 'hashed_password_here', 'HR', 0, GETDATE(), 1);
```

### Running the Application

1. Open `ContractMonthlyClaimSystem.sln` in Visual Studio
2. Restore NuGet packages
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)
5. Navigate to `https://localhost:7xxx`

**Default Login Credentials**:
- **HR**: hr@cmcs.com / Password123!

---

## Project Structure

```
ContractMonthlyClaimSystem/
├── Services/
│   ├── ClaimService.cs              # Core claim operations
│   ├── ClaimValidationService.cs    # Validation logic (180-hour rule)
│   ├── InvoiceGenerationService.cs  # PDF invoice generation
│   ├── ReportGenerationService.cs   # LINQ-based reporting
│   └── DataService.cs               # Database operations
├── Views/
│   ├── Account/
│   │   └── Login.cshtml             # Login page
│   ├── HR/
│   │   ├── Index.cshtml             # HR dashboard
│   │   ├── ManageUsers.cshtml       # User management
│   │   └── ViewInvoices.cshtml      # Generate invoices
│   ├── Lecturer/
│   │   ├── SubmitClaim.cshtml       # Submit claims (auto-calc)
│   │   ├── TrackClaim.cshtml        # Track claim status
│   │   └── ViewClaims.cshtml        # Claim history
│   ├── Coordinator/
│   │   └── ReviewClaims.cshtml      # First-level approval
│   └── Manager/
│       └── ReviewClaims.cshtml      # Final approval
├── wwwroot/
│   ├── js/
│   │   ├── claim-automation.js      # Auto-calculation logic
│   │   └── validation.js            # Client-side validation
│   └── css/
│       └── site.css                 # Custom styles
├── App_Data/                        # Legacy (now using EF Core)
├── Program.cs                       # Application startup
└── appsettings.json                 # Configuration
```

---

## Testing

### Unit Tests (`CMCS.Tests`)

1. **Validation Tests**:
   - ✅ Hours exceed 180 in a month
   - ✅ Negative hours validation
   - ✅ Missing required fields

2. **Claim Processing Tests**:
   - ✅ Auto-calculation accuracy
   - ✅ Status workflow (Pending → Approved → Rejected)

3. **Authorization Tests**:
   - ✅ Unauthorized access blocked
   - ✅ Role-based permissions enforced

**Run Tests**:
```bash
dotnet test CMCS.Tests/CMCS.Tests.csproj
```

---

## Version Control (GitHub)

### Commit History Guidelines
- ✅ **Minimum 10 commits** with descriptive messages
- ✅ Commit after each feature implementation
- ✅ Example messages:
  - "feat: Add HR user management functionality"
  - "fix: Remove monthly claim submission limit"
  - "refactor: Implement EF Core with SQL Server"
  - "docs: Update README with Part 3 changes"

### GitHub Repository Structure
```
├── README.md
├── .gitignore
├── ContractMonthlyClaimSystem.sln
├── ContractMonthlyClaimSystem/
├── CMCS.Tests/
├── Presentation.pptx
└── DatabaseSchema.sql
```

---

## PowerPoint Presentation Requirements

### Slide 1: Title
- Project name, student details, date

### Slide 2-3: Changes from Part 2 to Part 3
- HR role implementation
- Removed monthly claim limit
- EF Core database integration
- Enhanced validation

### Slide 4-8: Key Features (with Code Screenshots)
1. **HR User Management** - Show `ManageUsers.cshtml.cs` code
2. **Auto-Calculation** - Show JavaScript snippet from `claim-automation.js`
3. **Session Management** - Show `SessionHelper.cs` code
4. **Validation Service** - Show 180-hour validation logic
5. **PDF Generation** - Show LINQ query for invoice generation

### Slide 9: Architecture Diagram
- Show data flow: Lecturer → Coordinator → Manager → HR

### Slide 10: Conclusion
- Summary of improvements
- Future enhancements

---

## Video Demonstration (YouTube - Unlisted)

### Video Structure (10-15 minutes)

**Segment 1: HR Functionality (3-4 mins)**
1. Login as HR
2. Create a new lecturer account
3. Update hourly rate
4. Generate an invoice for a lecturer

**Segment 2: Lecturer Workflow (3-4 mins)**
1. Login as lecturer
2. Submit multiple claims in same month
3. Show auto-calculation feature
4. Demonstrate 180-hour validation
5. Track claim status

**Segment 3: Approval Workflow (3-4 mins)**
1. Login as Programme Coordinator
2. Review and approve a claim
3. Login as Academic Manager
4. Final approval of claim

**Segment 4: Security & Sessions (2-3 mins)**
1. Attempt to access unauthorized page (show 403 error)
2. Logout and login as different role
3. Show session management in action

---

## Known Issues & Future Enhancements

### Current Limitations
- PDF generation requires additional library (QuestPDF recommended)
- Email notifications not implemented
- Mobile responsiveness needs improvement

### Planned Enhancements
- Email notifications on claim status changes
- Dashboard analytics with charts
- Bulk claim processing for HR
- Advanced reporting with filters
- API endpoints for mobile app integration

---

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="QuestPDF" Version="2024.3.0" /> <!-- For PDF generation -->
```

### Database
- SQL Server 2019 or later
- LocalDB for development

---

## Security Considerations

1. **Password Hashing**: All passwords stored using BCrypt
2. **Session Timeout**: 30-minute inactivity timeout
3. **HTTPS**: Enforced in production
4. **SQL Injection**: Prevented by EF Core parameterized queries
5. **CSRF Protection**: Built-in Razor Pages anti-forgery tokens

---

## Support & Contact

For questions or issues:
- Create an issue on GitHub repository
- Email: support@cmcs.com
- Documentation: See inline code comments

---

## License

This project is an academic assignment for PROG6212 and is not licensed for commercial use.

---

## Acknowledgments

- Lecturer feedback incorporated from Part 2
- Bootstrap for UI framework
- Entity Framework Core for data access
- Microsoft documentation and tutorials

---

**Last Updated**: November 2024  
**Version**: 3.0  
**Course**: PROG6212 Final POE
