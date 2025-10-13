#!/bin/bash

# Seed sample data for Intune Management System
# This script creates sample users and policies for testing

API_BASE_URL="${API_BASE_URL:-http://localhost:5000/api}"

echo "================================================"
echo "Intune Management System - Data Seeding Script"
echo "================================================"
echo ""
echo "API URL: $API_BASE_URL"
echo ""

# Check if backend is running
echo "Checking if backend is running..."
if ! curl -s "$API_BASE_URL/../health" > /dev/null 2>&1; then
    echo "⚠️  Warning: Backend may not be running at $API_BASE_URL"
    echo "   Please start the backend before running this script."
    echo ""
fi

# Create test users
echo "Creating test users..."
echo ""

# Admin user
echo "Creating admin user..."
curl -X POST "$API_BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "email": "admin@example.com",
    "password": "Admin123!"
  }' 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✓ Admin user created (admin@example.com / Admin123!)"
else
    echo "✗ Failed to create admin user (may already exist)"
fi
echo ""

# Test user
echo "Creating test user..."
curl -X POST "$API_BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!"
  }' 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✓ Test user created (test@example.com / Test123!)"
else
    echo "✗ Failed to create test user (may already exist)"
fi
echo ""

# Login and get token
echo "Logging in to get access token..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Admin123!"
  }')

TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    echo "✗ Failed to login. Cannot create policies."
    echo "   Please ensure the backend is running and try again."
    exit 1
fi

echo "✓ Login successful"
echo ""

# Create sample policies
echo "Creating sample policies..."
echo ""

# Policy 1: Microsoft Office Deployment
echo "Creating Microsoft Office deployment policy..."
curl -X POST "$API_BASE_URL/policies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Microsoft Office 365 Deployment",
    "description": "Deploy Microsoft Office 365 ProPlus to managed devices",
    "policyType": "Application",
    "configuration": {
      "installCommand": "setup.exe /configure configuration.xml",
      "uninstallCommand": "setup.exe /configure uninstall.xml",
      "architecture": "x64",
      "minOSVersion": "10.0.0.0",
      "features": ["Word", "Excel", "PowerPoint", "Outlook"]
    }
  }' 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✓ Microsoft Office policy created"
else
    echo "✗ Failed to create Microsoft Office policy"
fi
echo ""

# Policy 2: Chrome Browser Deployment
echo "Creating Chrome browser deployment policy..."
curl -X POST "$API_BASE_URL/policies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Google Chrome Browser",
    "description": "Deploy Google Chrome browser with enterprise settings",
    "policyType": "Application",
    "configuration": {
      "installCommand": "chrome_installer.exe /silent /install",
      "uninstallCommand": "msiexec /x {CHROME_GUID} /quiet",
      "architecture": "x64",
      "minOSVersion": "10.0.0.0",
      "autoUpdate": true,
      "defaultSearchEngine": "google"
    }
  }' 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✓ Chrome browser policy created"
else
    echo "✗ Failed to create Chrome browser policy"
fi
echo ""

# Policy 3: Security Configuration
echo "Creating security configuration policy..."
curl -X POST "$API_BASE_URL/policies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Baseline Security Configuration",
    "description": "Apply baseline security settings to all devices",
    "policyType": "Configuration",
    "configuration": {
      "firewallEnabled": true,
      "antivirusEnabled": true,
      "encryptionRequired": true,
      "passwordComplexity": "high",
      "passwordMinLength": 12,
      "screenLockTimeout": 15,
      "usbStorageBlocked": true
    }
  }' 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✓ Security configuration policy created"
else
    echo "✗ Failed to create security configuration policy"
fi
echo ""

# Policy 4: Compliance Policy
echo "Creating compliance policy..."
curl -X POST "$API_BASE_URL/policies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Device Compliance - Windows 10",
    "description": "Ensure Windows 10 devices meet compliance requirements",
    "policyType": "Compliance",
    "configuration": {
      "minOSVersion": "10.0.19041.0",
      "bitLockerEnabled": true,
      "secureBootEnabled": true,
      "antivirusRequired": true,
      "firewallRequired": true,
      "passwordRequired": true,
      "jailbrokenDevicesBlocked": true
    }
  }' 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✓ Compliance policy created"
else
    echo "✗ Failed to create compliance policy"
fi
echo ""

# Policy 5: Custom Line-of-Business App
echo "Creating custom LOB app deployment policy..."
curl -X POST "$API_BASE_URL/policies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Custom LOB Application",
    "description": "Deploy custom line-of-business application",
    "policyType": "Application",
    "configuration": {
      "installCommand": "install.bat",
      "uninstallCommand": "uninstall.bat",
      "architecture": "x64",
      "minOSVersion": "10.0.0.0",
      "requiresReboot": false,
      "installBehavior": "system",
      "detectionMethod": "registry",
      "registryPath": "HKLM\\SOFTWARE\\CompanyName\\AppName",
      "registryValue": "Version",
      "registryExpectedValue": "1.0.0"
    }
  }' 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✓ Custom LOB app policy created"
else
    echo "✗ Failed to create custom LOB app policy"
fi
echo ""

echo "================================================"
echo "Data seeding completed!"
echo "================================================"
echo ""
echo "Test Accounts Created:"
echo "  1. admin@example.com / Admin123!"
echo "  2. test@example.com / Test123!"
echo ""
echo "Sample Policies Created:"
echo "  1. Microsoft Office 365 Deployment"
echo "  2. Google Chrome Browser"
echo "  3. Baseline Security Configuration"
echo "  4. Device Compliance - Windows 10"
echo "  5. Custom LOB Application"
echo ""
echo "You can now login at http://localhost:5173"
echo ""
