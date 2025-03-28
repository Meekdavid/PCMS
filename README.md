
```markdown
# Pension Contribution Management System

[![.NET Core](https://img.shields.io/badge/.NET-6.0-blue)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-blue)](https://www.microsoft.com/sql-server)
[![Swagger](https://img.shields.io/badge/Swagger-UI-green)](https://nlpcpcms.runasp.net/swagger/index.html)

A comprehensive pension management system built with .NET Core that handles member accounts, contributions, withdrawals, and benefit eligibility verification.

## Live Deployment

The API is currently deployed on MonsterASP.NET and can be accessed at:  
🔗 [https://nlpcpcms.runasp.net/swagger/index.html](http://nlpcpcms.runasp.net/swagger/index.html)

## Key Features

### Core Functionality
- **Member Management**: Registration, profile updates, and account management
- **Account Services**: Multiple account types (individual/employer-sponsored)
- **Contribution Processing**: Monthly and voluntary contributions with validation
- **Withdrawal System**: Pension benefit withdrawals with eligibility checks
- **Employer Portal**: Employer registration and employee management

### Advanced Features
- **Eligibility Verification**: Automated benefit eligibility assessment
- **Statement Generation**: PDF contribution statements for members
- **Audit Logging**: Comprehensive tracking of all system activities
- **Role-Based Access**: Secure access control for members, employers, and admins

## Technology Stack

### Backend
- **Framework**: .NET Core 6.0
- **Database**: Microsoft SQL Server 2019
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity with JWT

### Infrastructure
- **Hosting**: MonsterASP.NET
- **Storage**: Local file system (with Firebase storage adapter)
- **API Documentation**: Swagger/OpenAPI

## Database Schema

The system uses Microsoft SQL Server with the following core entities:

### Key Models
```csharp
// Account management
public class Account : BaseModel { ... }

// Member information
public class Member : IdentityUser<string>, IBaseModel { ... }

// Employer records
public class Employer : BaseModel { ... }

// Contribution tracking
public class Contribution : BaseModel { ... }

// Transaction processing
public class Transaction : BaseModel { ... }

// Eligibility rules engine
public class EligibilityRule : BaseModel { ... }

// Benefit eligibility tracking
public class BenefitEligibility : BaseModel { ... }
```

[View all models](#) (https://github.com/Meekdavid/PCMS/wiki)

## Deployment Guide

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+
- IIS (for Windows hosting)
- MonsterASP.NET account (or alternative hosting)

### Deployment Steps to MonsterASP.NET

1. **Prepare the Application**
   ```bash
   dotnet publish --configuration Release --output ./publish
   ```

2. **Database Setup**
   - Create SQL Server database on your hosting provider
   - Update connection string in `appsettings.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;"
     }
     ```

3. **Upload to MonsterASP.NET**
   - Compress the publish folder
   - Upload via MonsterASP.NET control panel
   - Set the appropriate permissions

4. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

5. **Configure Hosting**
   - Set the application pool to "No Managed Code"
   - Ensure proper .NET Core hosting bundle is installed

### Alternative Deployment Options

#### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "PensionContributionSystem.API.dll"]
```

#### Azure App Service
- Create new Web App
- Configure for .NET Core 6.0
- Deploy via GitHub Actions or FTP

## Development Setup

1. Clone the repository
   ```bash
   git clone https://github.com/Meekdavid/PCMS
   ```

2. Restore dependencies
   ```bash
   dotnet restore
   ```

3. Configure environment variables
   - Create `appsettings.Development.json`
   - Set your local database connection string

4. Run database migrations
   ```bash
   dotnet ef database update
   ```

5. Start the application
   ```bash
   dotnet run
   ```

## API Documentation

The system provides comprehensive API documentation through Swagger UI. After deployment, access the documentation at:

`/swagger/index.html`

Example endpoints:
- `POST /api/accounts` - Create new pension account
- `POST /api/contributions` - Process new contribution
- `GET /api/members/{memberId}` - Retrieve member details
[View all endpoints](#) (https://github.com/Meekdavid/PCMS/wiki)

## Performance Considerations

1. **Database Optimization**
   - Indexed frequently queried columns
   - Implemented pagination for large datasets
   - Used stored procedures for complex queries

2. **Caching Strategy**
   - Memory caching for static data
   - Response caching for frequently accessed endpoints

3. **Asynchronous Processing**
   - Background services for long-running tasks
   - Async/await pattern throughout the application

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Support

For any issues or questions, please contact:  
📧 [mbokodavid@gmail.com](mailto:mbokodavid@gmail)
```
