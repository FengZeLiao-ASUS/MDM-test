# Quick Start Guide

This guide will help you get the Intune Management System up and running quickly.

## Prerequisites Check

Before starting, ensure you have:
- [ ] Node.js v18+ installed (`node --version`)
- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] Azure AD App Registration created
- [ ] Admin consent granted for Graph API permissions

## Step 1: Clone and Setup

```bash
git clone https://github.com/FengZeLiao-ASUS/MDM-test.git
cd MDM-test
```

## Step 2: Configure Backend

1. Edit `backend/IntuneManagement/appsettings.json`:
```json
{
  "AzureAd": {
    "TenantId": "YOUR_TENANT_ID_HERE",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
  }
}
```

2. Start the backend:
```bash
cd backend/IntuneManagement
dotnet restore
dotnet run
```

The backend will be available at `http://localhost:5000`

## Step 3: Configure Frontend

1. Edit `frontend/.env`:
```
VITE_API_BASE_URL=http://localhost:5000/api
```

2. Start the frontend:
```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173`

## Step 4: Create First User

Open a new terminal and create a test user:

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "email": "admin@example.com",
    "password": "Admin123!"
  }'
```

## Step 5: Login

1. Open browser to `http://localhost:5173`
2. Login with:
   - Email: `admin@example.com`
   - Password: `Admin123!`

## Step 6: Test the System

### View Devices
1. After login, you'll be on the Dashboard
2. Click "Refresh" to load devices from Intune
3. If no devices appear, verify your Azure AD configuration

### Create a Policy
1. Navigate to "Policies" tab
2. Click "Create Policy"
3. Fill in:
   - Name: "Test App Deployment"
   - Description: "Test application"
   - Type: "Application"
4. Click "Create"

### Deploy a Policy
1. Click "Deploy to Intune" on your policy
2. The system will generate the necessary files
3. Check the deployment message for status

## Troubleshooting

### Backend won't start
- Ensure port 5000 is not in use
- Check `appsettings.json` for correct format
- Verify .NET 8 SDK is installed

### Frontend won't start
- Delete `node_modules` and run `npm install` again
- Ensure port 5173 is available
- Check `.env` file exists and has correct API URL

### Can't see devices
- Verify Azure AD credentials in `appsettings.json`
- Ensure your Azure AD app has the required permissions
- Check that admin consent has been granted
- Review backend logs for errors

### Authentication fails
- Ensure a user has been created via the register endpoint
- Check password matches requirements
- Verify backend is running and accessible

## Next Steps

1. **Security**: Update password hashing to use BCrypt
2. **IntuneWinAppUtil**: Download and configure the tool path
3. **Production**: Set up proper authentication with JWT tokens
4. **Database**: Migrate from SQLite to SQL Server or PostgreSQL
5. **Monitoring**: Add Application Insights or similar logging

## Common Commands

### Backend
```bash
# Build
dotnet build

# Run
dotnet run

# Run with watch (auto-reload)
dotnet watch run

# Create database migration
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```

### Frontend
```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

## Docker Deployment (Optional)

If you prefer using Docker:

```bash
# Build and start all services
docker-compose up --build

# Stop all services
docker-compose down

# View logs
docker-compose logs -f
```

## Support

For issues or questions:
1. Check the main README.md for detailed documentation
2. Review error messages in browser console and backend logs
3. Verify all configuration files are correctly set up
