# Just Broadcast - Control Center

A modern, role-based Blazor WebAssembly application for managing broadcast playout operations with JWT authentication and DevExpress UI components.

## Features

- **JWT-based Authentication**: Secure login with token-based authentication
- **Role-based Access Control**: Four user roles (Supervisor, Administrator, Operator, Viewer)
- **Modern Dashboard**: Real-time monitoring of playout operations
- **DevExpress Components**: Professional UI with DevExpress Blazor components
- **Responsive Design**: Works on desktop and mobile devices

## User Roles

### Supervisor
- Full system access
- Can manage playouts, users, and settings
- Access to all features

### Administrator
- Can manage playouts, users, and settings
- Access to administrative features

### Operator
- Can manage playouts
- Limited administrative access

### Viewer
- Read-only access
- Can view dashboard and playout information

## Project Structure

```
JustBroadcast/
├── Models/                      # Data models
│   ├── DashboardData.cs        # Dashboard data structures
│   ├── LoginRequest.cs         # Login request model
│   ├── LoginResponse.cs        # Login response model
│   ├── User.cs                 # User model
│   └── UserRole.cs             # User role enum
├── Services/                    # Business logic services
│   ├── AuthService.cs          # Authentication service
│   ├── CustomAuthStateProvider.cs  # Custom auth state provider
│   ├── IAuthService.cs         # Auth service interface
│   └── AuthorizationHelper.cs  # Role-based authorization helpers
├── Pages/                       # Razor pages
│   ├── Dashboard.razor         # Main dashboard page
│   ├── Login.razor             # Login page
│   └── ...
├── Layout/                      # Layout components
│   ├── MainLayout.razor        # Main application layout
│   └── NavMenu.razor           # Navigation menu
├── Shared/                      # Shared components
│   └── RedirectToLogin.razor   # Login redirect component
└── wwwroot/                     # Static files
    ├── appsettings.json        # Application configuration
    └── index.html              # Main HTML file
```

## Configuration

### API Settings

Update `wwwroot/appsettings.json` with your API endpoints:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7000",
    "LoginEndpoint": "/api/auth/login",
    "DashboardEndpoint": "/api/dashboard"
  }
}
```

## API Integration

### Authentication Endpoint

The application expects a login endpoint that accepts:

**Request:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "token": "jwt_token_string",
  "username": "string",
  "email": "string",
  "role": "Supervisor|Administrator|Operator|Viewer",
  "expiration": "2024-12-31T23:59:59Z"
}
```

### JWT Token Claims

The JWT token should include the following claims:
- `unique_name` or `sub` or `name`: Username
- `role`: User role (Supervisor, Administrator, Operator, or Viewer)
- `email`: User email (optional)

### Dashboard Endpoint

The application can fetch dashboard data from your API:

**Endpoint:** `/api/dashboard`

**Response:** See `Models/DashboardData.cs` for the complete structure.

If the API endpoint is not available, the application will use mock data for demonstration purposes.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
2. Navigate to the project directory
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

### Running the Application

1. Start the development server:
   ```bash
   dotnet run
   ```

2. Open your browser and navigate to the displayed URL (typically `https://localhost:5001`)

3. Login with credentials from your API

### Building for Production

```bash
dotnet publish -c Release
```

The published files will be in `bin/Release/net10.0/publish/wwwroot/`

## Authentication Flow

1. User enters credentials on the login page
2. Application sends credentials to the API login endpoint
3. API validates credentials and returns JWT token
4. Application stores token in local storage
5. Token is added to HTTP Authorization header for subsequent requests
6. User is redirected to the dashboard
7. Navigation menu shows options based on user role

## Role-Based Features

The application uses extension methods on `ClaimsPrincipal` for role checking:

- `HasRole(UserRole role)`: Check if user has specific role
- `HasAnyRole(params UserRole[] roles)`: Check if user has any of the specified roles
- `CanManagePlayouts()`: Supervisor, Administrator, or Operator
- `CanManageUsers()`: Supervisor or Administrator
- `CanManageSettings()`: Supervisor or Administrator
- `IsSupervisor()`: Supervisor only
- `IsAdministrator()`: Administrator only
- `IsOperator()`: Operator only

## Dashboard Features

The dashboard displays:
- **Stats Cards**: Playouts, Channels, Users, Media Assets, Alerts
- **Active Playouts Table**: Real-time status of all playouts
- **System Resources**: CPU, GPU, RAM usage with charts
- **Error Feed**: Recent errors and warnings
- **Error Frequency Chart**: 7-day error trends
- **Alerts**: Important system notifications

## Customization

### Changing Colors

Main color scheme is defined in CSS files:
- Primary Blue: `#4A90E2`
- Dark Background: `#1a1f37`
- Panel Background: `#252d47`
- Border Color: `#1e2539`

### Adding New Pages

1. Create a new `.razor` file in the `Pages` folder
2. Add the `@page` directive with the route
3. Add `@attribute [Authorize]` for protected pages
4. Add role-based checks using `AuthorizeView` or `@if (User.HasRole(...))`
5. Add navigation menu item in `Layout/NavMenu.razor`

### Modifying Roles

1. Update `Models/UserRole.cs` enum
2. Update `Services/AuthorizationHelper.cs` with new role checks
3. Update navigation menu in `Layout/NavMenu.razor`

## Security Considerations

- JWT tokens are stored in browser local storage
- Tokens are included in Authorization header for API requests
- Protected routes require authentication
- Role-based authorization on navigation items
- HTTPS is recommended for production

## Troubleshooting

### Login fails with "Invalid username or password"
- Check that your API endpoint is correct in `appsettings.json`
- Verify the API is running and accessible
- Check browser console for network errors

### DevExpress components not rendering
- Ensure DevExpress CSS and JS are loaded in `index.html`
- Check browser console for errors
- Verify DevExpress.Blazor NuGet package is installed

### Navigation menu items not showing
- Verify user role is correctly set in JWT token
- Check role claim name matches expected format
- Verify authorization helper methods in `AuthorizationHelper.cs`

## Technologies Used

- Blazor WebAssembly (.NET 10)
- DevExpress Blazor Components (v25.2.3)
- JWT Authentication
- Blazored.LocalStorage
- System.IdentityModel.Tokens.Jwt

## License

Copyright © 2025 Just Broadcast

## Support

For issues or questions, please contact your administrator.
