# MDM-test - Intune Management System

A complete system for managing Microsoft Intune devices with a React frontend and .NET backend, featuring authentication, device monitoring, policy management, and .intunewin package deployment.

## 📖 Documentation

| Document | Description |
|----------|-------------|
| **[🚀 GETTING_STARTED.md](GETTING_STARTED.md)** | **Start here! Quick 10-minute setup guide** |
| [QUICKSTART.md](QUICKSTART.md) | Detailed step-by-step setup instructions |
| [API_DOCUMENTATION.md](API_DOCUMENTATION.md) | Complete API reference with examples |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture and design decisions |
| [INTUNE_WINAPPUTIL.md](INTUNE_WINAPPUTIL.md) | IntuneWinAppUtil integration guide |
| [IMPROVEMENTS.md](IMPROVEMENTS.md) | Security issues, concepts, and improvements |

## 🎯 Quick Start

```bash
# 1. Configure Azure AD (get your tenant & client IDs)
# 2. Update backend/IntuneManagement/appsettings.json
# 3. Update frontend/.env with Azure AD credentials

# 4. Start Backend
cd backend/IntuneManagement
dotnet run

# 5. Start Frontend (new terminal)
cd frontend
npm install && npm run dev

# 6. Open http://localhost:5173 and sign in with Microsoft
```

**For complete setup instructions, see [GETTING_STARTED.md](GETTING_STARTED.md)**

## Architecture Overview

This project implements a separated frontend and backend architecture:

- **Frontend**: React with TypeScript, using Vite as the build tool
  - Vite dev server configured with proxy to forward `/api` requests to backend
- **Backend**: .NET 8 Web API with Entity Framework Core
- **Authentication**: Microsoft Authentication Library (MSAL) for Azure AD authentication
- **Database**: SQLite for policy storage
- **Integration**: Microsoft Graph API for Intune device management

## Features

### 1. User Authentication
- Users sign in using their Microsoft account via Azure AD
- MSAL (Microsoft Authentication Library) handles authentication flow
- Access tokens are automatically acquired for Microsoft Graph API access

### 2. Device Status Monitoring
- View all managed devices from Microsoft Intune
- Display device information including:
  - Device name and operating system
  - OS version and compliance state
  - Management agent status
  - Last sync date/time
  - Associated user principal name

### 3. Policy Management
- Create custom policies with configuration options
- View all existing policies
- Delete policies when no longer needed
- Store policy configurations in JSON format

### 4. Application Deployment
- Select a policy to deploy
- Backend automatically generates necessary files:
  - `install.bat` - Installation script
  - `uninstall.bat` - Uninstallation script
  - `configuration.json` - Policy configuration
  - `detect.ps1` - Detection script for Intune
- Integration with Microsoft's IntuneWinAppUtil tool to create `.intunewin` packages
- Deploy applications to Intune via Graph API

## Prerequisites

- **Node.js** (v18 or higher)
- **.NET 8 SDK**
- **Azure AD Application** with the following permissions:
  - `User.Read`
  - `DeviceManagementManagedDevices.Read.All`
  - `DeviceAppManagement.ReadWrite.All`

## Setup Instructions

### 1. Azure AD Configuration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to Azure Active Directory > App registrations
3. Create a new application registration:
   - Name: "Intune Management System"
   - Supported account types: "Accounts in this organizational directory only"
   - Redirect URI: 
     - Type: Single-page application (SPA)
     - URI: `http://localhost:5173`
4. Note the **Application (client) ID** and **Directory (tenant) ID**
5. Create a client secret under "Certificates & secrets" (for backend API access)
6. Grant the following API permissions under "API permissions":
   - Microsoft Graph > Delegated permissions (for frontend MSAL):
     - `User.Read`
   - Microsoft Graph > Application permissions (for backend):
     - `DeviceManagementManagedDevices.Read.All`
     - `DeviceAppManagement.ReadWrite.All`
   - Request admin consent for these permissions

### 2. Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend/IntuneManagement
   ```

2. Update `appsettings.json` with your Azure AD credentials:
   ```json
   {
     "AzureAd": {
       "TenantId": "YOUR_TENANT_ID",
       "ClientId": "YOUR_CLIENT_ID",
       "ClientSecret": "YOUR_CLIENT_SECRET"
     }
   }
   ```

3. (Optional) Configure IntuneWinAppUtil path if you have it installed:
   ```json
   {
     "IntunePackage": {
       "WorkingDirectory": "/tmp/intune-packages",
       "UtilPath": "/path/to/IntuneWinAppUtil.exe"
     }
   }
   ```

4. Restore dependencies and run the backend:
   ```bash
   dotnet restore
   dotnet run
   ```

   The backend API will start on `http://localhost:5136` (HTTP) or `https://localhost:7204` (HTTPS)

### 3. Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Create or update `.env` file with your Azure AD configuration:
   ```
   VITE_API_BASE_URL=http://localhost:5136/api
   VITE_AZURE_CLIENT_ID=YOUR_CLIENT_ID
   VITE_AZURE_TENANT_ID=YOUR_TENANT_ID
   VITE_REDIRECT_URI=http://localhost:5173
   ```

4. Start the development server:
   ```bash
   npm run dev
   ```

   The frontend will start on `http://localhost:5173`

### 4. IntuneWinAppUtil Setup (Optional)

To enable `.intunewin` package creation:

1. Download IntuneWinAppUtil from [Microsoft's GitHub](https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool)
2. Extract the tool to a location on your system
3. Update the backend `appsettings.json` with the path to `IntuneWinAppUtil.exe`

If IntuneWinAppUtil is not available, the system will still create the necessary source files but won't generate the `.intunewin` package.

## Usage

### Logging In

1. Open `http://localhost:5173` in your browser
2. Click "Sign in with Microsoft"
3. Authenticate with your Microsoft/Azure AD account
4. Upon successful authentication, you'll be redirected to the dashboard

### Viewing Devices

1. After logging in, the dashboard displays all managed devices from Intune
2. Click "Refresh" to reload device data
3. Device information includes compliance status, OS details, and last sync time

### Creating and Deploying Policies

1. Navigate to the "Policies" tab
2. Click "Create Policy"
3. Fill in the policy details:
   - Name: A descriptive name for the policy
   - Description: What the policy does
   - Policy Type: Application, Configuration, or Compliance
4. Click "Create" to save the policy
5. To deploy a policy:
   - Click "Deploy to Intune" on any policy card
   - The system will generate necessary files and create a `.intunewin` package
   - If configured, it will automatically upload to Intune

## API Endpoints

### Devices
- `GET /api/devices` - Get all managed devices from Intune

### Policies
- `GET /api/policies` - Get all policies
- `GET /api/policies/{id}` - Get a specific policy
- `POST /api/policies` - Create a new policy
- `DELETE /api/policies/{id}` - Delete a policy
- `POST /api/policies/deploy` - Deploy a policy to Intune

**Note**: All API endpoints require a valid Azure AD access token in the Authorization header.

## Project Structure

```
MDM-test/
├── backend/
│   └── IntuneManagement/
│       ├── Controllers/          # API controllers
│       ├── Services/             # Business logic services
│       ├── Models/               # Data models
│       ├── DTOs/                 # Data transfer objects
│       ├── Data/                 # Database context
│       └── appsettings.json      # Configuration
├── frontend/
│   └── src/
│       ├── components/           # Reusable React components
│       ├── pages/                # Page components
│       ├── services/             # API service layer
│       ├── config/               # Configuration files
│       └── types/                # TypeScript type definitions
└── README.md
```

## Key Concepts & Improvements

### Security Considerations

1. **Azure AD Authentication**: The system uses MSAL for secure authentication with Azure AD. Tokens are managed automatically with proper expiration handling.

2. **HTTPS**: Always use HTTPS in production environments.

3. **Secret Management**: Never commit sensitive credentials to source control. Use Azure Key Vault or environment variables.

4. **Token Security**: Access tokens are stored in sessionStorage and cleared on logout. For enhanced security, consider implementing additional token protection mechanisms.

### Database

The system uses SQLite for policy storage. For production:
- Consider using SQL Server or PostgreSQL
- Implement proper database migrations
- Add connection pooling and retry logic

### Error Handling

The current implementation has basic error handling. Consider:
- Implementing global error handlers
- Adding detailed logging with Application Insights
- Implementing retry policies for Graph API calls

### Graph API Integration

The Graph API integration is simplified. For production:
- Implement proper token caching
- Handle rate limiting and throttling
- Add support for batching requests
- Implement proper error handling for API calls

### IntuneWinAppUtil Integration

The tool integration is basic. Improvements:
- Add support for different file types (MSI, EXE, scripts)
- Implement proper validation of generated packages
- Add support for dependency management
- Implement versioning for applications

### Policy Configuration

The current policy configuration is JSON-based. Consider:
- Adding a policy template system
- Implementing policy validation
- Adding support for policy versioning
- Creating a visual policy editor

### Testing

Add comprehensive testing:
- Unit tests for services and controllers
- Integration tests for API endpoints
- End-to-end tests for critical workflows
- Load testing for Graph API integration

### Deployment

For production deployment:
- Containerize with Docker
- Use Docker Compose for orchestration
- Implement CI/CD pipelines
- Add health check endpoints
- Implement proper logging and monitoring

## Troubleshooting

### Backend Issues

1. **Graph API authentication errors**: Verify Azure AD credentials in `appsettings.json`
2. **CORS errors**: Ensure the frontend URL is in the CORS policy

### Frontend Issues

1. **API connection errors**: Verify `VITE_API_BASE_URL` in `.env`
2. **MSAL authentication errors**: Verify Azure AD configuration in `.env` and ensure redirect URI is registered in Azure portal
3. **Build errors**: Delete `node_modules` and `package-lock.json`, then run `npm install`
4. **TypeScript errors**: Run `npm run build` to check for type errors

## Contributing

This is a test/demo project for understanding Intune integration concepts. Feel free to extend and improve it for your needs.

## License

This project is for educational purposes. Please review Microsoft's terms of service for Graph API usage.
