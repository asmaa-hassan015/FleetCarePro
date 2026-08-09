### Getting Started

**Prerequisites**
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or a full instance)
- Visual Studio 2022 (or VS Code + C# Dev Kit)

**1. Configure the connection string**

Open `appsettings.json` and update `ConnectionStrings:DefaultConnection` if your SQL Server instance is not the local default:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=FleetCareProDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**2. Apply the EF Core migrations**

From the project folder, run:

```bash
dotnet ef database update
```

This creates the `FleetCareProDb` database with all tables (Vehicles, ServiceRecords, ServiceLineItems, ServiceCenters, ServiceCategories, VendorServices, AuditLogs, and the ASP.NET Identity tables).

> If `dotnet ef` isn't recognized, install it once with `dotnet tool install --global dotnet-ef`.

**3. Run the app**

```bash
dotnet run
```

Or press **F5** in Visual Studio. On first run, `SeedData` automatically creates the **Admin**, **FleetManager**, and **Driver** roles plus the three test accounts listed below — no manual setup needed.

The app is available at:
- `https://localhost:7158`
- `http://localhost:5294`

**Maintenance Mode**

Set `"IsMaintenanceMode": true` in `appsettings.json` and restart the app to redirect all traffic to the maintenance page. Set it back to `false` to resume normal access.

### Key Features
- Vehicle, service center, service category, and maintenance record management.
- **EF Core Code First** with different relationships between entities.
- Authentication and authorization using **ASP.NET Core Identity**.
- 3 roles: **Admin / FleetManager / Driver**.
- Custom **VIN validation**.
- Vehicle image and maintenance invoice uploads.
- **ViewModels + AutoMapper**.
- **LINQ, Transactions, and Pagination**.
- **AJAX** for searching and filtering.
- **Audit Log** for tracking user actions.
- **Maintenance Mode Middleware**.
- Responsive UI using **Bootstrap 5**, Partial Views, and View Components.

### Login Credentials
|            Role    |        Email           |   Password  |
|--------------------|------------------------|-------------|
|      **Admin**     |  `admin@fleetcare.com` | `Admin@123` |
| **FleetManager**   | `manager@fleetcare.com`|`Manager@123`|
|     **Driver**     | `driver@fleetcare.com` | `Driver@123`|

### Role Permissions
- **Admin:** Full access, including user and role management.
- **FleetManager:** Manage vehicles, service centers, service categories, and maintenance records.
- **Driver:** View assigned vehicles and their maintenance history.

### Technologies Used
**C# – .NET 8 – ASP.NET Core MVC – Entity Framework Core – SQL Server – ASP.NET Core Identity – AutoMapper – Bootstrap 5 – LINQ – AJAX**