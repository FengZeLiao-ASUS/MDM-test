@echo off
REM Seed sample data for Intune Management System
REM This script creates sample users and policies for testing

setlocal enabledelayedexpansion

set API_BASE_URL=http://localhost:5000/api
if not "%API_BASE_URL%"=="" set API_BASE_URL=%API_BASE_URL%

echo ================================================
echo Intune Management System - Data Seeding Script
echo ================================================
echo.
echo API URL: %API_BASE_URL%
echo.

REM Check if curl is available
where curl >nul 2>nul
if %errorlevel% neq 0 (
    echo Error: curl is not available. Please install curl or use Git Bash to run seed-data.sh
    exit /b 1
)

echo Creating test users...
echo.

REM Admin user
echo Creating admin user...
curl -X POST "%API_BASE_URL%/auth/register" -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"email\":\"admin@example.com\",\"password\":\"Admin123!\"}" >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Admin user created ^(admin@example.com / Admin123!^)
) else (
    echo [SKIP] Failed to create admin user ^(may already exist^)
)
echo.

REM Test user
echo Creating test user...
curl -X POST "%API_BASE_URL%/auth/register" -H "Content-Type: application/json" -d "{\"username\":\"testuser\",\"email\":\"test@example.com\",\"password\":\"Test123!\"}" >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Test user created ^(test@example.com / Test123!^)
) else (
    echo [SKIP] Failed to create test user ^(may already exist^)
)
echo.

REM Login and get token
echo Logging in to get access token...
curl -s -X POST "%API_BASE_URL%/auth/login" -H "Content-Type: application/json" -d "{\"email\":\"admin@example.com\",\"password\":\"Admin123!\"}" > temp_login.json
findstr "accessToken" temp_login.json >nul
if %errorlevel% neq 0 (
    echo [ERROR] Failed to login. Cannot create policies.
    echo         Please ensure the backend is running and try again.
    del temp_login.json
    exit /b 1
)

REM Extract token (simplified - in production use proper JSON parsing)
for /f "tokens=2 delims=:," %%a in ('findstr "accessToken" temp_login.json') do set TOKEN=%%a
set TOKEN=%TOKEN:"=%
set TOKEN=%TOKEN: =%
del temp_login.json

echo [OK] Login successful
echo.

echo Creating sample policies...
echo.

REM Policy 1: Microsoft Office
echo Creating Microsoft Office deployment policy...
curl -X POST "%API_BASE_URL%/policies" -H "Content-Type: application/json" -H "Authorization: Bearer %TOKEN%" -d "{\"name\":\"Microsoft Office 365 Deployment\",\"description\":\"Deploy Microsoft Office 365 ProPlus to managed devices\",\"policyType\":\"Application\",\"configuration\":{\"installCommand\":\"setup.exe /configure configuration.xml\",\"uninstallCommand\":\"setup.exe /configure uninstall.xml\",\"architecture\":\"x64\",\"minOSVersion\":\"10.0.0.0\"}}" >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Microsoft Office policy created
) else (
    echo [SKIP] Failed to create Microsoft Office policy
)
echo.

REM Policy 2: Chrome Browser
echo Creating Chrome browser deployment policy...
curl -X POST "%API_BASE_URL%/policies" -H "Content-Type: application/json" -H "Authorization: Bearer %TOKEN%" -d "{\"name\":\"Google Chrome Browser\",\"description\":\"Deploy Google Chrome browser with enterprise settings\",\"policyType\":\"Application\",\"configuration\":{\"installCommand\":\"chrome_installer.exe /silent /install\",\"architecture\":\"x64\",\"minOSVersion\":\"10.0.0.0\"}}" >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Chrome browser policy created
) else (
    echo [SKIP] Failed to create Chrome browser policy
)
echo.

REM Policy 3: Security Configuration
echo Creating security configuration policy...
curl -X POST "%API_BASE_URL%/policies" -H "Content-Type: application/json" -H "Authorization: Bearer %TOKEN%" -d "{\"name\":\"Baseline Security Configuration\",\"description\":\"Apply baseline security settings to all devices\",\"policyType\":\"Configuration\",\"configuration\":{\"firewallEnabled\":true,\"antivirusEnabled\":true,\"encryptionRequired\":true}}" >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Security configuration policy created
) else (
    echo [SKIP] Failed to create security configuration policy
)
echo.

echo ================================================
echo Data seeding completed!
echo ================================================
echo.
echo Test Accounts Created:
echo   1. admin@example.com / Admin123!
echo   2. test@example.com / Test123!
echo.
echo Sample Policies Created:
echo   1. Microsoft Office 365 Deployment
echo   2. Google Chrome Browser
echo   3. Baseline Security Configuration
echo.
echo You can now login at http://localhost:5173
echo.

endlocal
