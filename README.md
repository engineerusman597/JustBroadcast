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
    "BaseUrl": "http://178.222.112.105:5016",
    "SignalRHub": "http://178.222.112.105:5016/broadcastHub",
    "UseMockAuth": false,
    "PlayoutsEndpoint": "/api/Playouts/short",
    "ChannelsCountEndpoint": "/api/Channels/count",
    "ErrorsLastWeekEndpoint": "/api/Errors/lastweek",
    "AssetsCountEndpoint": "/api/Assets/count",
    "UsersCountEndpoint": "/api/Userlists/count",
    "SystemSettingsTelemetryEndpoint": "/api/Systemsettings/settelemetrycontrol"
  }
}
```

**Important:** Update the `BaseUrl` and `SignalRHub` to match your backend server location.

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

Before running this project on another developer's laptop, ensure:

- **.NET 10.0 SDK** or later
  - Download from: https://dotnet.microsoft.com/download
  - Verify: `dotnet --version`

- **Visual Studio 2022** (recommended) or **VS Code**
  - Visual Studio 2022: Community, Professional, or Enterprise
  - VS Code: Install C# Dev Kit extension

- **Modern web browser**
  - Chrome, Edge, Firefox, or Safari (latest versions)

### Installation Steps

1. **Copy the project folder** to the developer's laptop

2. **Navigate to the project directory:**
   ```bash
   cd JustBroadcast/JustBroadcast
   ```

3. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```
   This downloads all required dependencies including DevExpress Blazor components.

4. **Verify/Update API configuration:**

   Open `wwwroot/appsettings.json` and update if needed:
   ```json
   {
     "ApiSettings": {
       "BaseUrl": "http://178.222.112.105:5016",
       "SignalRHub": "http://178.222.112.105:5016/broadcastHub"
     }
   }
   ```

5. **Build the project:**
   ```bash
   dotnet build
   ```
   You should see "Build succeeded" message.

### Running the Application

**Option A: Using Visual Studio 2022**
1. Open `JustBroadcast.sln` in Visual Studio
2. Press **F5** or click "Start" button
3. Application opens in default browser

**Option B: Using Command Line**
```bash
dotnet run
```
Application will be available at: `https://localhost:5150`

**Option C: Using Visual Studio Code**
1. Open the project folder in VS Code
2. Press **F5** to start debugging
3. Or use terminal: `dotnet run`

### First Login

Use your backend API credentials:
- Username: `admin` or `supervisor`
- Password: (provided by backend team)

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

The published files will be in `./publish/wwwroot/` folder.

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
- **Active Playouts Table**: Real-time status of all playouts (ON AIR/OFFLINE)
- **Active Users Panel**: Live tracking of connected users:
  - Web Dashboard users
  - Remote Control clients
  - Scheduler clients
  - CG Control clients
- **System Resources**: CPU, GPU, RAM usage with real-time area charts
- **Error Feed**: Recent errors and warnings
- **Error Frequency Chart**: Last 7 days error trends
- **Alerts**: Important system notifications
- **Telemetry Control**: Toggle system telemetry ON/OFF

### Real-Time Features (SignalR)

The application uses SignalR for real-time updates:
- **Playout Status Changes**: Automatic updates when playouts go online/offline
- **Active Users Tracking**: See who's connected in real-time across all applications
- **System Metrics**: Live CPU/GPU/RAM monitoring with historical charts
- **Error Notifications**: Instant error alerts
- **Multi-User Sync**: All connected dashboards stay synchronized

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

### SignalR Connection Issues
**Symptoms:** "SignalR connection failed" or "HTTP Authentication failed"

**Solutions:**
1. Verify `SignalRHub` URL in `appsettings.json` is correct
2. Ensure backend server is running and accessible
3. Check that JWT token is being sent (look for `[SignalR]` logs in browser console)
4. Verify firewall/antivirus is not blocking WebSocket connections

### Active Users Not Showing in Real-Time
**Symptoms:** Users don't appear immediately when they log in

**Solutions:**
1. Open browser console (F12) and check for `[Users]` log messages
2. Verify SignalR connection is established (look for "SignalR connected successfully")
3. Ensure multiple users are logged in from different browsers
4. Refresh the page to reload the initial snapshot

### System Metrics Not Updating
**Symptoms:** CPU/GPU/RAM charts are static

**Solutions:**
1. Check browser console for `[Metrics]` log messages
2. Verify backend is sending Metrics commands via SignalR
3. Ensure telemetry is enabled (green "Telemetry ON" button)

### DevExpress Evaluation Warning
**Symptoms:** Build warning "DX1000: For evaluation purposes only"

**Solution:** This is normal for evaluation mode. The application will work correctly. To remove the warning, register or purchase a DevExpress license.

### Build Errors
**Symptoms:** Cannot build or restore packages

**Solutions:**
1. Ensure .NET 10.0 SDK is installed: `dotnet --version`
2. Delete `bin` and `obj` folders, then run `dotnet restore`
3. Clear NuGet cache: `dotnet nuget locals all --clear`

### Navigation menu items not showing
- Verify user role is correctly set in JWT token
- Check role claim name matches expected format
- Verify authorization helper methods in `AuthorizationHelper.cs`

## Development Tips

### Browser Console Debugging

The application provides extensive console logging. Press **F12** to open DevTools:

- `[SignalR]` - SignalR connection and commands
- `[Users]` - Active users tracking
- `[Metrics]` - System metrics updates
- `[API]` - API responses

### Testing Real-Time Features

1. Open the application in **two different browsers** (e.g., Chrome and Edge)
2. Log in with different users (admin in one, supervisor in another)
3. Both users should appear in the **Active Users** panel on both dashboards
4. When one user logs out, they should disappear from the other dashboard immediately

### Hot Reload

When running in development mode (`dotnet run`), the application supports hot reload:
- Changes to `.razor` files reload the UI automatically
- Changes to `.cs` files may require a full restart
- Changes to `.css` files reload styles automatically

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
