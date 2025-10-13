# System Architecture

This document explains the architecture and design decisions of the Intune Management System.

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           User's Web Browser                             │
│                         (React + TypeScript)                             │
└────────────────┬────────────────────────────────────────────────────────┘
                 │
                 │ HTTP/HTTPS
                 │ REST API
                 │
┌────────────────▼────────────────────────────────────────────────────────┐
│                        Backend API Server                                │
│                    (.NET 8 Web API + EF Core)                           │
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────┐               │
│  │   Auth       │  │   Policy     │  │   Device       │               │
│  │   Service    │  │   Service    │  │   Service      │               │
│  └──────────────┘  └──────────────┘  └────────────────┘               │
│                                                                          │
│  ┌──────────────┐  ┌──────────────────────────────────────┐            │
│  │   Intune     │  │   Graph API Service                  │            │
│  │   Package    │  │   (Microsoft Graph Client)           │            │
│  │   Service    │  └──────────────────────────────────────┘            │
│  └──────────────┘                                                       │
└────────────────┬────────────────────────────────┬───────────────────────┘
                 │                                │
                 │                                │ HTTPS
                 │ File System                    │ OAuth 2.0 + Client Secret
                 │                                │
         ┌───────▼────────┐              ┌───────▼─────────────────────┐
         │   SQLite DB    │              │   Microsoft Graph API       │
         │   (User Data,  │              │   (Azure AD + Intune)       │
         │    Policies)   │              │                             │
         └────────────────┘              └─────────────────────────────┘
                                                      │
                                                      │
                                         ┌────────────▼──────────────┐
                                         │  Microsoft Intune Portal  │
                                         │  (Device Management)      │
                                         └───────────────────────────┘
```

## Component Architecture

### Frontend (React + TypeScript)

```
frontend/
├── src/
│   ├── pages/                  # Page components
│   │   ├── Login.tsx          # Authentication page
│   │   ├── Dashboard.tsx      # Device status dashboard
│   │   └── Policies.tsx       # Policy management
│   │
│   ├── components/            # Reusable UI components
│   │
│   ├── services/              # API communication
│   │   └── apiService.ts     # HTTP client wrapper
│   │
│   ├── config/                # Configuration
│   │   ├── authConfig.ts     # MSAL configuration
│   │   └── apiConfig.ts      # API endpoints
│   │
│   └── types/                 # TypeScript types
│       └── index.ts          # Type definitions
│
└── .env                       # Environment variables
```

**Key Technologies**:
- React 18 with TypeScript
- Vite for build tooling
- React Router for navigation
- Axios for HTTP requests
- MSAL for Azure AD authentication

### Backend (.NET 8 Web API)

```
backend/IntuneManagement/
├── Controllers/               # API endpoints
│   ├── AuthController.cs     # /api/auth/*
│   ├── DevicesController.cs  # /api/devices/*
│   └── PoliciesController.cs # /api/policies/*
│
├── Services/                  # Business logic
│   ├── AuthService.cs        # User authentication
│   ├── PolicyService.cs      # Policy CRUD
│   ├── GraphApiService.cs    # Microsoft Graph integration
│   └── IntunePackageService.cs # .intunewin creation
│
├── Models/                    # Data models
│   ├── User.cs
│   └── Policy.cs
│
├── DTOs/                      # Data transfer objects
│   ├── AuthDTOs.cs
│   ├── PolicyDTOs.cs
│   └── DeviceDTOs.cs
│
├── Data/                      # Database context
│   └── AppDbContext.cs
│
└── Program.cs                 # Application startup
```

**Key Technologies**:
- ASP.NET Core 8
- Entity Framework Core
- Microsoft.Graph SDK
- Microsoft.Identity.Web
- SQLite database

## Data Flow Diagrams

### 1. User Authentication Flow

```
┌──────┐                                                   ┌──────────┐
│ User │                                                   │ Database │
└───┬──┘                                                   └────┬─────┘
    │                                                           │
    │ 1. Enter credentials (email, password)                   │
    ├──────────────────────────────────────────────────►       │
    │                                                           │
    │              2. Hash password & query DB                 │
    │                                          ◄───────────────┤
    │                                                           │
    │ 3. Return JWT token + user info                          │
    │ ◄──────────────────────────────────────                 │
    │                                                           │
    │ 4. Store token in sessionStorage                         │
    │                                                           │
    │ 5. Redirect to dashboard                                 │
    │                                                           │
```

### 2. Device Status Retrieval Flow

```
┌──────┐          ┌─────────┐          ┌────────────┐          ┌────────┐
│ User │          │ Backend │          │ Graph API  │          │ Intune │
└───┬──┘          └────┬────┘          └─────┬──────┘          └───┬────┘
    │                  │                     │                      │
    │ 1. Request       │                     │                      │
    │    devices       │                     │                      │
    ├─────────────────►│                     │                      │
    │                  │                     │                      │
    │                  │ 2. Authenticate     │                      │
    │                  │    with client      │                      │
    │                  │    secret           │                      │
    │                  ├────────────────────►│                      │
    │                  │                     │                      │
    │                  │ 3. Request          │                      │
    │                  │    managed devices  │                      │
    │                  ├────────────────────►│                      │
    │                  │                     │                      │
    │                  │                     │ 4. Query devices     │
    │                  │                     ├─────────────────────►│
    │                  │                     │                      │
    │                  │                     │ 5. Return data       │
    │                  │                     │◄─────────────────────┤
    │                  │                     │                      │
    │                  │ 6. Device list      │                      │
    │                  │◄────────────────────┤                      │
    │                  │                     │                      │
    │ 7. Display       │                     │                      │
    │    devices       │                     │                      │
    │◄─────────────────┤                     │                      │
    │                  │                     │                      │
```

### 3. Policy Deployment Flow

```
┌──────┐     ┌─────────┐     ┌──────────┐     ┌────────────┐     ┌────────┐
│ User │     │ Backend │     │ IntuneWin│     │ Graph API  │     │ Intune │
│      │     │ Service │     │ AppUtil  │     │            │     │        │
└───┬──┘     └────┬────┘     └────┬─────┘     └─────┬──────┘     └───┬────┘
    │             │               │                  │                 │
    │ 1. Select   │               │                  │                 │
    │    policy & │               │                  │                 │
    │    deploy   │               │                  │                 │
    ├────────────►│               │                  │                 │
    │             │               │                  │                 │
    │             │ 2. Create     │                  │                 │
    │             │    files      │                  │                 │
    │             │    (bat, json,│                  │                 │
    │             │     ps1)      │                  │                 │
    │             │               │                  │                 │
    │             │ 3. Package    │                  │                 │
    │             │    files      │                  │                 │
    │             ├──────────────►│                  │                 │
    │             │               │                  │                 │
    │             │ 4. Return     │                  │                 │
    │             │    .intunewin │                  │                 │
    │             │◄──────────────┤                  │                 │
    │             │               │                  │                 │
    │             │ 5. Upload     │                  │                 │
    │             │    package    │                  │                 │
    │             ├──────────────────────────────────►│                 │
    │             │               │                  │                 │
    │             │               │                  │ 6. Create app   │
    │             │               │                  ├────────────────►│
    │             │               │                  │                 │
    │             │               │                  │ 7. Assign       │
    │             │               │                  ├────────────────►│
    │             │               │                  │                 │
    │             │ 8. Return     │                  │                 │
    │             │    app ID     │                  │                 │
    │             │◄──────────────────────────────────┤                 │
    │             │               │                  │                 │
    │ 9. Show     │               │                  │                 │
    │    success  │               │                  │                 │
    │◄────────────┤               │                  │                 │
    │             │               │                  │                 │
```

## Security Architecture

### Authentication & Authorization

1. **Local Authentication**:
   - User credentials stored in SQLite
   - Passwords hashed with SHA256 (upgrade to BCrypt recommended)
   - Simple token-based auth (upgrade to JWT recommended)

2. **Azure AD Integration**:
   - Service principal with client secret
   - Application permissions for Graph API
   - Token management handled by Microsoft.Identity.Web

### API Security

```
Request Flow:
┌─────────────────┐
│   API Request   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  CORS Policy    │  ← Validate origin
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Auth Middleware│  ← Validate token
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Controller     │  ← Process request
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Response     │
└─────────────────┘
```

## Database Schema

```sql
-- Users table
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    LastLogin DATETIME,
    IsActive BOOLEAN NOT NULL DEFAULT 1
);

-- Policies table
CREATE TABLE Policies (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    PolicyType TEXT NOT NULL,
    Configuration TEXT NOT NULL,  -- JSON string
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    IsActive BOOLEAN NOT NULL DEFAULT 1
);
```

## Deployment Architecture

### Development Environment

```
┌──────────────────────────────────────┐
│     Developer Machine                 │
│                                       │
│  ┌─────────┐        ┌──────────┐    │
│  │ Frontend│        │ Backend  │    │
│  │         │        │          │    │
│  │ :5173   │◄──────►│ :5000    │    │
│  └─────────┘        └──────────┘    │
│                                       │
│  SQLite DB                            │
│  Azure AD (test tenant)               │
└──────────────────────────────────────┘
```

### Production Environment (Recommended)

```
                    ┌─────────────┐
                    │   Azure AD  │
                    │   + Intune  │
                    └──────▲──────┘
                           │
┌──────────────────────────┼───────────────────────────┐
│                          │                            │
│  ┌───────────────┐       │        ┌────────────┐    │
│  │  Azure Front  │       │        │  Azure     │    │
│  │  Door / CDN   │◄──────┼───────►│  API       │    │
│  │               │       │        │  Management│    │
│  └───────┬───────┘       │        └─────┬──────┘    │
│          │               │              │            │
│  ┌───────▼──────┐        │      ┌───────▼──────┐    │
│  │  React App   │        │      │  .NET API    │    │
│  │  (Static)    │        │      │  (App Service│    │
│  │              │        └─────►│   or AKS)    │    │
│  └──────────────┘                └───────┬──────┘    │
│                                          │            │
│                                  ┌───────▼──────┐    │
│                                  │  Azure SQL   │    │
│                                  │  Database    │    │
│                                  └──────────────┘    │
└───────────────────────────────────────────────────────┘
```

## Design Decisions

### Why These Technologies?

1. **.NET 8**: 
   - Native Microsoft Graph SDK support
   - Strong typing and performance
   - Cross-platform compatibility
   - Excellent Azure integration

2. **React + TypeScript**:
   - Type safety for large codebases
   - Rich ecosystem
   - MSAL React library available
   - Modern developer experience

3. **SQLite**:
   - Zero configuration
   - File-based (easy to backup)
   - Perfect for development/testing
   - Easy migration to production DB

4. **Graph API**:
   - Official Microsoft solution
   - Comprehensive Intune coverage
   - Well-documented
   - Active development

### Separation of Concerns

1. **Frontend**: Pure presentation layer
   - No business logic
   - API calls through service layer
   - Type-safe interfaces

2. **Backend**: Business logic + data
   - RESTful API design
   - Service layer pattern
   - Repository pattern (EF Core)
   - Dependency injection

3. **External Services**: Microsoft services
   - Graph API for Intune
   - Azure AD for identity
   - Clear boundaries via interfaces

## Scalability Considerations

### Current Limitations

1. Single-server deployment
2. SQLite (not suitable for high concurrency)
3. No caching layer
4. Synchronous API calls

### Scaling Path

1. **Database**: Migrate to Azure SQL or PostgreSQL
2. **Caching**: Add Redis for token caching
3. **Load Balancing**: Use Azure Load Balancer or App Gateway
4. **Background Jobs**: Use Azure Functions for async tasks
5. **Monitoring**: Add Application Insights
6. **CDN**: Serve frontend via Azure CDN

## Future Enhancements

1. **Real-time Updates**: SignalR for live device status
2. **Reporting**: Power BI integration for analytics
3. **Audit Trail**: Comprehensive logging of all actions
4. **Multi-tenancy**: Support multiple organizations
5. **Role-Based Access**: Granular permissions
6. **Notification System**: Email/SMS alerts
7. **Mobile App**: Native mobile client
8. **Bulk Operations**: Deploy to multiple devices at once
