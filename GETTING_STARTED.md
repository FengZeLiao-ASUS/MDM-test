# 🚀 Getting Started - Intune Management System

Welcome! This guide will get you up and running in **10 minutes**.

## ⚡ Quick Setup (3 Steps)

### Step 1: Prerequisites (2 minutes)

Make sure you have these installed:
```bash
# Check Node.js (need v18+)
node --version

# Check .NET (need v8+)
dotnet --version

# If not installed:
# - Node.js: https://nodejs.org
# - .NET 8: https://dotnet.microsoft.com/download
```

### Step 2: Azure AD Setup (5 minutes)

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**:
   - Name: `Intune Management System`
   - Supported account types: `Accounts in this organizational directory only`
   - Redirect URI: 
     - Type: `Single-page application (SPA)`
     - URI: `http://localhost:5173`
4. Note your **Application (client) ID** and **Directory (tenant) ID**
5. Go to **Certificates & secrets** > Create a **New client secret** (for backend)
6. Go to **API permissions** > Add these permissions:
   - `Microsoft Graph` > `Delegated permissions` (for frontend MSAL):
     - `User.Read`
   - `Microsoft Graph` > `Application permissions` (for backend):
     - `DeviceManagementManagedDevices.Read.All`
     - `DeviceAppManagement.ReadWrite.All`
   - Click **Grant admin consent**

### Step 3: Start the System (3 minutes)

**Terminal 1 - Backend:**
```bash
cd backend/IntuneManagement

# Update appsettings.json with your Azure AD values:
# - TenantId: your-tenant-id
# - ClientId: your-client-id
# - ClientSecret: your-client-secret

dotnet restore
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd frontend

# Create .env file with your Azure AD values:
# VITE_API_BASE_URL=http://localhost:5136/api
# VITE_AZURE_CLIENT_ID=your-client-id
# VITE_AZURE_TENANT_ID=your-tenant-id
# VITE_REDIRECT_URI=http://localhost:5173

npm install
npm run dev
```

## 🎉 You're Ready!

Open your browser: **http://localhost:5173**

Click **Sign in with Microsoft** and authenticate with your Azure AD account.

## 📋 What You Can Do Now

### View Devices
1. After login, you'll see the Dashboard
2. Click **Refresh** to load devices from Intune
3. View device compliance, OS info, and sync status

### Create a Policy
1. Go to **Policies** tab
2. Click **Create Policy**
3. Fill in:
   ```
   Name: Test Application
   Description: My first policy
   Type: Application
   ```
4. Click **Create**

### Deploy to Intune
1. Click **Deploy to Intune** on your policy
2. System will:
   - Generate install/uninstall scripts
   - Create `.intunewin` package (if tool configured)
   - Upload to Intune (if permissions granted)

## 🎯 Next Steps

### Add Sample Data
```bash
# Linux/Mac
./seed-data.sh

# Windows
seed-data.bat
```

This creates:
- 2 test users (admin & test)
- 5 sample policies

### Configure IntuneWinAppUtil (Optional)

1. Download from [GitHub](https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool)
2. Place at `C:\Tools\IntuneWinAppUtil\IntuneWinAppUtil.exe`
3. Update `appsettings.json`:
   ```json
   {
     "IntunePackage": {
       "UtilPath": "C:\\Tools\\IntuneWinAppUtil\\IntuneWinAppUtil.exe"
     }
   }
   ```

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| [README.md](README.md) | Complete project overview |
| [QUICKSTART.md](QUICKSTART.md) | Detailed setup instructions |
| [API_DOCUMENTATION.md](API_DOCUMENTATION.md) | API reference |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System design |
| [IMPROVEMENTS.md](IMPROVEMENTS.md) | Security & enhancements |

## ❓ Troubleshooting

### "Cannot fetch devices"
- Check Azure AD credentials in `appsettings.json`
- Verify admin consent was granted
- Check backend logs for errors

### "Login failed"
- Ensure user was created (run register curl command)
- Check backend is running on port 5136
- Verify email/password are correct

### "Frontend won't start"
- Delete `node_modules` folder
- Run `npm install` again
- Check port 5173 is available

### "Backend won't start"
- Check .NET 8 SDK is installed
- Verify port 5136 is not in use
- Check `appsettings.json` is valid JSON

## 🔧 Common Tasks

### Add a New User via API
```bash
curl -X POST http://localhost:5136/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

### Test API Endpoints
```bash
# Login
TOKEN=$(curl -s -X POST http://localhost:5136/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin123!"}' \
  | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

# Get Devices
curl -X GET http://localhost:5136/api/devices \
  -H "Authorization: Bearer $TOKEN"

# Get Policies
curl -X GET http://localhost:5136/api/policies \
  -H "Authorization: Bearer $TOKEN"
```

### Reset Database
```bash
cd backend/IntuneManagement
rm intune_management.db
dotnet run  # Database will be recreated
```

## 🐳 Docker (Alternative Setup)

If you prefer Docker:

```bash
# Build and start everything
docker-compose up --build

# Access:
# Frontend: http://localhost:3000
# Backend: http://localhost:5136
```

## 💡 Tips

1. **Use Swagger UI**: Visit `http://localhost:5136/swagger` to test APIs
2. **Check Backend Logs**: Backend terminal shows all API calls and errors
3. **Browser DevTools**: Open Console (F12) to see frontend logs
4. **Sample Policies**: Use seed-data script for ready-made examples
5. **Read IMPROVEMENTS.md**: Learn about security and production recommendations

## 🎓 Learning Resources

- [Microsoft Graph API Docs](https://docs.microsoft.com/en-us/graph/)
- [Intune App Management](https://docs.microsoft.com/en-us/mem/intune/apps/)
- [MSAL Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [React Documentation](https://react.dev/)

## 🆘 Get Help

1. Check the troubleshooting section above
2. Review error messages in browser console and backend logs
3. Verify all configuration files are correct
4. Ensure Azure AD permissions are properly set
5. Check that all prerequisites are installed

## ✅ Checklist

- [ ] Node.js and .NET installed
- [ ] Azure AD app registration created
- [ ] Admin consent granted for API permissions
- [ ] Backend `appsettings.json` updated with Azure AD values
- [ ] Backend running on port 5136
- [ ] Frontend running on port 5173
- [ ] Test user created
- [ ] Successfully logged in
- [ ] Can see login page

## 🎊 Success!

You now have a working Intune management system! 

**What's Next?**
1. Create real policies for your organization
2. Deploy applications to Intune
3. Monitor device compliance
4. Implement production improvements from IMPROVEMENTS.md

---

**Need more help?** Check out the comprehensive [README.md](README.md) or [QUICKSTART.md](QUICKSTART.md) for detailed information.

**Ready for production?** Review [IMPROVEMENTS.md](IMPROVEMENTS.md) for security and scalability recommendations.
