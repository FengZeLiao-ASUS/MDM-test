# API Documentation

This document describes all available API endpoints for the Intune Management System.

## Base URL

```
Development: http://localhost:5136/api
Production: https://your-domain.com/api
```

## Authentication

Most endpoints require authentication. Include the access token in the Authorization header:

```http
Authorization: Bearer YOUR_ACCESS_TOKEN
```

## Response Format

All responses are in JSON format with the following structure:

**Success Response:**
```json
{
  "data": { ... },
  "message": "Success message"
}
```

**Error Response:**
```json
{
  "message": "Error message",
  "error": "Detailed error description"
}
```

---

## Authentication Endpoints

### Register User

Create a new user account.

**Endpoint:** `POST /api/auth/register`

**Authentication:** Not required

**Request Body:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** `200 OK`
```json
{
  "message": "Registration successful"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid input or user already exists
```json
{
  "message": "User already exists or registration failed"
}
```

**Example:**
```bash
curl -X POST http://localhost:5136/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "SecurePassword123!"
  }'
```

---

### Login

Authenticate a user and receive an access token.

**Endpoint:** `POST /api/auth/login`

**Authentication:** Not required

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com"
  }
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid credentials
```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

- `400 Bad Request` - Missing required fields
```json
{
  "success": false,
  "message": "Email and password are required"
}
```

**Example:**
```bash
curl -X POST http://localhost:5136/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePassword123!"
  }'
```

---

## Device Endpoints

### Get All Devices

Retrieve all managed devices from Microsoft Intune.

**Endpoint:** `GET /api/devices`

**Authentication:** Required

**Query Parameters:** None

**Response:** `200 OK`
```json
{
  "devices": [
    {
      "id": "device-guid-1",
      "deviceName": "DESKTOP-ABC123",
      "operatingSystem": "Windows",
      "osVersion": "10.0.19045",
      "complianceState": "Compliant",
      "managementAgent": "MDM",
      "lastSyncDateTime": "2024-01-15T10:30:00Z",
      "userPrincipalName": "user@contoso.com"
    },
    {
      "id": "device-guid-2",
      "deviceName": "LAPTOP-XYZ789",
      "operatingSystem": "Windows",
      "osVersion": "11.0.22621",
      "complianceState": "NonCompliant",
      "managementAgent": "MDM",
      "lastSyncDateTime": "2024-01-15T09:15:00Z",
      "userPrincipalName": "user2@contoso.com"
    }
  ],
  "totalCount": 2
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Graph API error or configuration issue
```json
{
  "message": "Error fetching devices",
  "error": "Error details..."
}
```

**Example:**
```bash
curl -X GET http://localhost:5136/api/devices \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

## Policy Endpoints

### Get All Policies

Retrieve all active policies.

**Endpoint:** `GET /api/policies`

**Authentication:** Required

**Query Parameters:** None

**Response:** `200 OK`
```json
[
  {
    "id": 1,
    "name": "Microsoft Office 365",
    "description": "Deploy Office 365 to all devices",
    "policyType": "Application",
    "configuration": {
      "installCommand": "setup.exe /configure config.xml",
      "uninstallCommand": "setup.exe /configure uninstall.xml",
      "architecture": "x64",
      "minOSVersion": "10.0.0.0"
    },
    "createdAt": "2024-01-10T14:30:00Z"
  },
  {
    "id": 2,
    "name": "Security Baseline",
    "description": "Apply security settings",
    "policyType": "Configuration",
    "configuration": {
      "firewallEnabled": true,
      "antivirusEnabled": true,
      "encryptionRequired": true
    },
    "createdAt": "2024-01-11T09:00:00Z"
  }
]
```

**Example:**
```bash
curl -X GET http://localhost:5136/api/policies \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

### Get Policy by ID

Retrieve a specific policy.

**Endpoint:** `GET /api/policies/{id}`

**Authentication:** Required

**URL Parameters:**
- `id` (integer) - Policy ID

**Response:** `200 OK`
```json
{
  "id": 1,
  "name": "Microsoft Office 365",
  "description": "Deploy Office 365 to all devices",
  "policyType": "Application",
  "configuration": {
    "installCommand": "setup.exe /configure config.xml",
    "uninstallCommand": "setup.exe /configure uninstall.xml",
    "architecture": "x64",
    "minOSVersion": "10.0.0.0"
  },
  "createdAt": "2024-01-10T14:30:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Policy doesn't exist
```json
{
  "message": "Policy not found"
}
```

**Example:**
```bash
curl -X GET http://localhost:5136/api/policies/1 \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

### Create Policy

Create a new policy.

**Endpoint:** `POST /api/policies`

**Authentication:** Required

**Request Body:**
```json
{
  "name": "Google Chrome",
  "description": "Deploy Chrome browser",
  "policyType": "Application",
  "configuration": {
    "installCommand": "chrome_installer.exe /silent",
    "uninstallCommand": "msiexec /x {CHROME_GUID} /quiet",
    "architecture": "x64",
    "minOSVersion": "10.0.0.0",
    "autoUpdate": true
  }
}
```

**Response:** `201 Created`
```json
{
  "id": 3,
  "name": "Google Chrome",
  "description": "Deploy Chrome browser",
  "policyType": "Application",
  "configuration": {
    "installCommand": "chrome_installer.exe /silent",
    "uninstallCommand": "msiexec /x {CHROME_GUID} /quiet",
    "architecture": "x64",
    "minOSVersion": "10.0.0.0",
    "autoUpdate": true
  },
  "createdAt": "2024-01-15T12:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid input
```json
{
  "message": "Policy name is required"
}
```

**Example:**
```bash
curl -X POST http://localhost:5136/api/policies \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Google Chrome",
    "description": "Deploy Chrome browser",
    "policyType": "Application",
    "configuration": {
      "installCommand": "chrome_installer.exe /silent",
      "architecture": "x64"
    }
  }'
```

---

### Delete Policy

Delete (soft delete) a policy.

**Endpoint:** `DELETE /api/policies/{id}`

**Authentication:** Required

**URL Parameters:**
- `id` (integer) - Policy ID

**Response:** `204 No Content`

**Error Responses:**
- `404 Not Found` - Policy doesn't exist
```json
{
  "message": "Policy not found"
}
```

**Example:**
```bash
curl -X DELETE http://localhost:5136/api/policies/1 \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

### Deploy Policy

Deploy a policy to Microsoft Intune.

**Endpoint:** `POST /api/policies/deploy`

**Authentication:** Required

**Request Body:**
```json
{
  "policyId": 1,
  "targetGroup": "All Devices",
  "parameters": {
    "priority": "high",
    "restartBehavior": "allow"
  }
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "Package created successfully",
  "intunewinFilePath": "/tmp/intune-packages/abc-123/output/install.intunewin",
  "intuneApplicationId": "app-guid-from-intune"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid policy ID or deployment failed
```json
{
  "success": false,
  "message": "Policy not found"
}
```

- `500 Internal Server Error` - Deployment error
```json
{
  "success": false,
  "message": "Error: Failed to create .intunewin file"
}
```

**Example:**
```bash
curl -X POST http://localhost:5136/api/policies/deploy \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "policyId": 1,
    "targetGroup": "All Devices",
    "parameters": {}
  }'
```

---

## Data Types

### User
```typescript
interface User {
  id: number;
  username: string;
  email: string;
}
```

### Device
```typescript
interface Device {
  id: string;
  deviceName: string;
  operatingSystem: string;
  osVersion: string;
  complianceState: string;
  managementAgent: string;
  lastSyncDateTime?: string;
  userPrincipalName: string;
}
```

### Policy
```typescript
interface Policy {
  id: number;
  name: string;
  description: string;
  policyType: string;  // "Application" | "Configuration" | "Compliance"
  configuration: Record<string, any>;
  createdAt: string;
}
```

---

## Error Codes

| Status Code | Description |
|------------|-------------|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 404 | Not Found - Resource doesn't exist |
| 500 | Internal Server Error |

---

## Rate Limiting

Currently, there are no rate limits implemented. In production, consider:
- 100 requests per minute per user
- 1000 requests per hour per IP

---

## Testing the API

### Using cURL

```bash
# Login and save token
TOKEN=$(curl -s -X POST http://localhost:5136/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin123!"}' \
  | grep -o '"accessToken":"[^"]*"' \
  | cut -d'"' -f4)

# Use token for authenticated requests
curl -X GET http://localhost:5136/api/devices \
  -H "Authorization: Bearer $TOKEN"
```

### Using Postman

1. Import the collection (if available)
2. Set environment variable `baseUrl` to `http://localhost:5136/api`
3. Login and save `accessToken` to environment
4. Use `{{accessToken}}` in Authorization header

### Using the IntuneManagement.http file

If using Visual Studio or VS Code with REST Client extension:

```http
### Variables
@baseUrl = http://localhost:5136/api
@token = your-token-here

### Login
POST {{baseUrl}}/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin123!"
}

### Get Devices
GET {{baseUrl}}/devices
Authorization: Bearer {{token}}

### Create Policy
POST {{baseUrl}}/policies
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Test Policy",
  "description": "Test",
  "policyType": "Application",
  "configuration": {}
}
```

---

## Swagger/OpenAPI

The API includes Swagger UI for interactive documentation:

**Development:** http://localhost:5136/swagger

The Swagger UI provides:
- Interactive API testing
- Request/response examples
- Schema definitions
- Try it out functionality

---

## Best Practices

1. **Always validate input** on both client and server
2. **Use HTTPS** in production
3. **Store tokens securely** (use httpOnly cookies in production)
4. **Handle errors gracefully** with proper error messages
5. **Log all API calls** for audit purposes
6. **Version your API** (e.g., `/api/v1/...`)
7. **Document changes** in a changelog

---

## Changelog

### Version 1.0.0 (Current)
- Initial API release
- User authentication
- Device management
- Policy CRUD operations
- Policy deployment
