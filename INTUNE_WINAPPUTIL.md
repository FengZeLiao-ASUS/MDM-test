# IntuneWinAppUtil Integration Guide

This guide explains how to integrate and use Microsoft's IntuneWinAppUtil tool with this system.

## What is IntuneWinAppUtil?

IntuneWinAppUtil is Microsoft's official command-line tool that prepares Win32 apps for deployment through Microsoft Intune. It packages application files into a `.intunewin` format that can be uploaded to Intune.

## Downloading IntuneWinAppUtil

1. Visit the official Microsoft repository:
   https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool

2. Download the latest release (typically `IntuneWinAppUtil.exe`)

3. Place it in a known location, for example:
   - Windows: `C:\Tools\IntuneWinAppUtil\IntuneWinAppUtil.exe`
   - Linux/Mac (with Wine): `/opt/IntuneWinAppUtil/IntuneWinAppUtil.exe`

## Configuration

Update the backend `appsettings.json` with the tool path:

```json
{
  "IntunePackage": {
    "WorkingDirectory": "/tmp/intune-packages",
    "UtilPath": "C:\\Tools\\IntuneWinAppUtil\\IntuneWinAppUtil.exe"
  }
}
```

## How It Works in This System

When you deploy a policy through the UI:

1. **Backend generates source files**:
   - `install.bat` - Installation script
   - `uninstall.bat` - Uninstallation script
   - `configuration.json` - Policy configuration
   - `detect.ps1` - Detection script

2. **IntuneWinAppUtil packages the files**:
   ```bash
   IntuneWinAppUtil.exe -c <source_folder> -s install.bat -o <output_folder>
   ```

3. **Creates `.intunewin` file**:
   - Contains all source files in encrypted format
   - Includes metadata for Intune
   - Ready for upload to Intune portal

4. **Optional automatic upload**:
   - System attempts to upload via Graph API
   - Creates application entry in Intune
   - Assigns to specified groups

## Manual Usage Example

If you want to manually use IntuneWinAppUtil:

```bash
# Navigate to the tool directory
cd C:\Tools\IntuneWinAppUtil

# Package an application
IntuneWinAppUtil.exe -c "C:\Apps\MyApp" -s "setup.exe" -o "C:\Output"

# Parameters:
# -c : Source folder containing your app files
# -s : Setup file (installer) - must be in source folder
# -o : Output folder where .intunewin will be created
# -q : Quiet mode (optional)
```

## Example: Packaging Chrome Browser

### 1. Create source folder structure:

```
C:\Apps\Chrome\
├── chrome_installer.exe
├── install.bat
├── uninstall.bat
└── configuration.json
```

### 2. Create install.bat:

```batch
@echo off
echo Installing Google Chrome...
chrome_installer.exe /silent /install
if %errorlevel% equ 0 (
    echo Installation successful
    exit /b 0
) else (
    echo Installation failed
    exit /b 1
)
```

### 3. Run IntuneWinAppUtil:

```bash
IntuneWinAppUtil.exe -c "C:\Apps\Chrome" -s "install.bat" -o "C:\Output"
```

### 4. Result:

```
C:\Output\
└── install.intunewin
```

## Understanding Generated Files

### install.bat
The main installation script that Intune will execute:
```batch
@echo off
echo Installing policy: [Policy Name]
echo Configuration file: configuration.json

REM Parse configuration and apply settings
powershell -ExecutionPolicy Bypass -File "%~dp0apply-config.ps1"

echo Installation completed
exit /b 0
```

### uninstall.bat
Script for removing the application:
```batch
@echo off
echo Uninstalling policy: [Policy Name]

REM Remove applied settings
echo Uninstallation completed
exit /b 0
```

### configuration.json
Contains the policy configuration in JSON format:
```json
{
  "name": "Microsoft Office 365",
  "version": "1.0",
  "installCommand": "setup.exe /configure configuration.xml",
  "settings": {
    "architecture": "x64",
    "minOSVersion": "10.0.0.0"
  }
}
```

### detect.ps1
PowerShell script to detect if the app is installed:
```powershell
# Detection script for [Policy Name]
$configFile = Join-Path $PSScriptRoot "configuration.json"
if (Test-Path $configFile) {
    Write-Host "Policy is installed"
    exit 0
} else {
    exit 1
}
```

## Troubleshooting

### Tool Not Found

**Error**: `IntuneWinAppUtil not found`

**Solution**: 
- Verify the tool path in `appsettings.json`
- Ensure the file exists at the specified location
- On Linux/Mac, ensure Wine is installed for running .exe files

### Access Denied

**Error**: `Access denied when running IntuneWinAppUtil`

**Solution**:
- Run backend with administrator privileges (Windows)
- Check file permissions on the tool
- Ensure working directory is writable

### Invalid Package

**Error**: `.intunewin file created but invalid`

**Solution**:
- Ensure setup file (-s parameter) exists in source folder
- Verify source folder contains all necessary files
- Check that setup file name matches exactly (case-sensitive)

### No Detection Method

**Error**: `Application deployed but not detected`

**Solution**:
- Verify detect.ps1 logic is correct
- Test detection script manually on a target machine
- Check Intune logs for detection failures

## Advanced Usage

### Custom Setup Files

You can package any installer type:

```bash
# MSI installer
IntuneWinAppUtil.exe -c "C:\Apps\MyApp" -s "setup.msi" -o "C:\Output"

# PowerShell script
IntuneWinAppUtil.exe -c "C:\Apps\MyApp" -s "install.ps1" -o "C:\Output"

# Multiple files
IntuneWinAppUtil.exe -c "C:\Apps\MyApp" -s "launcher.exe" -o "C:\Output"
```

### Detection Rules in Intune

After uploading, configure detection in Intune portal:

1. **File Detection**:
   - Path: `%ProgramFiles%\YourApp`
   - File: `app.exe`
   - Check: File or folder exists

2. **Registry Detection**:
   - Path: `HKLM\SOFTWARE\YourApp`
   - Value: `Version`
   - Type: String
   - Operator: Equals
   - Value: `1.0.0`

3. **Script Detection** (using detect.ps1):
   - Upload the detection script
   - Set expected output

### Return Codes

Ensure your install scripts return proper exit codes:
- `0` = Success
- `3010` = Success, reboot required
- Other = Failure

Example:
```batch
@echo off
setup.exe /silent
if %errorlevel% equ 0 (
    exit /b 0
) else if %errorlevel% equ 3010 (
    exit /b 3010
) else (
    exit /b 1
)
```

## Integration with Graph API

After creating the `.intunewin` file, the system can automatically:

1. Upload content to Azure Storage (via Graph API)
2. Create Win32LobApp in Intune
3. Configure install/uninstall commands
4. Set requirements and detection rules
5. Assign to groups

This happens automatically when you click "Deploy to Intune" in the UI.

## Best Practices

1. **Test locally first**: Always test installation scripts on a test machine
2. **Use silent installers**: Ensure installers support silent/unattended mode
3. **Include dependencies**: Package all required files in source folder
4. **Version control**: Include version information in configuration
5. **Logging**: Add logging to install/uninstall scripts for troubleshooting
6. **Error handling**: Implement proper error handling and return codes
7. **Detection logic**: Keep detection scripts simple and reliable

## Resources

- [Official IntuneWinAppUtil Documentation](https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool)
- [Win32 App Management in Intune](https://docs.microsoft.com/en-us/mem/intune/apps/apps-win32-app-management)
- [Graph API for Intune](https://docs.microsoft.com/en-us/graph/api/resources/intune-apps-win32lobapp)
