# Contract Monthly Claim System (CMCS)

## PROG6212 Programming 2B - Part 2 Submission

A comprehensive ASP.NET Core MVC web application for managing monthly claims submission and approval workflow for independent contractor lecturers.

### Features Implemented

✅ **One-Click Claim Submission** - Simple, intuitive form for lecturers  
✅ **Role-Based Approval Workflow** - Coordinator and Manager dashboards  
✅ **Document Upload System** - Secure file storage with validation  
✅ **Real-Time Status Tracking** - Transparent claim progression  
✅ **Unit Testing** - Comprehensive test coverage  
✅ **Version Control** - Proper Git workflow with descriptive commits  

### Technology Stack

- **Backend**: ASP.NET Core 6.0 MVC
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery, AJAX
- **Testing**: xUnit, InMemory Database
- **Version Control**: Git & GitHub

### User Roles & Credentials

- **Lecturer**: Self-registration through application
- **Coordinator**: `coordinator@cmcs.com` / `Coordinator123!`
- **Academic Manager**: `manager@cmcs.com` / `Manager123!`

### Project Structure
CMCS-Project/
├── Controllers/ # MVC Controllers
├── Models/ # Data Models & View Models
├── Views/ # Razor Views
├── Data/ # DbContext & Migrations
├── Services/ # Business Logic Services
├── Areas/ # Identity Pages
└── ContractMonthlyClaimSystem.Tests/ # Unit Tests


### Setup Instructions

1. Clone repository
2. Restore NuGet packages
3. Update database: `Update-Database`
4. Run application
5. Use provided credentials for testing

### Commit History

The project demonstrates proper version control with descriptive commits showing the development progression.