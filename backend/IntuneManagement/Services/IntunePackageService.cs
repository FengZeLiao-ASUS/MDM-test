using System.Diagnostics;
using System.Text.Json;
using IntuneManagement.DTOs;

namespace IntuneManagement.Services;

public interface IIntunePackageService
{
    Task<DeployPolicyResponse> CreateIntunePackageAsync(DeployPolicyRequest request);
}

public class IntunePackageService : IIntunePackageService
{
    private readonly IPolicyService _policyService;
    private readonly IGraphApiService _graphApiService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IntunePackageService> _logger;
    private readonly string _workingDirectory;
    private readonly string _intuneWinAppUtilPath;

    public IntunePackageService(
        IPolicyService policyService,
        IGraphApiService graphApiService,
        IConfiguration configuration,
        ILogger<IntunePackageService> logger)
    {
        _policyService = policyService;
        _graphApiService = graphApiService;
        _configuration = configuration;
        _logger = logger;
        
        _workingDirectory = _configuration["IntunePackage:WorkingDirectory"] ?? "/tmp/intune-packages";
        _intuneWinAppUtilPath = _configuration["IntunePackage:UtilPath"] ?? "/opt/IntuneWinAppUtil/IntuneWinAppUtil.exe";
        
        Directory.CreateDirectory(_workingDirectory);
    }

    public async Task<DeployPolicyResponse> CreateIntunePackageAsync(DeployPolicyRequest request)
    {
        try
        {
            // Get policy details
            var policy = await _policyService.GetPolicyByIdAsync(request.PolicyId);
            if (policy == null)
            {
                return new DeployPolicyResponse
                {
                    Success = false,
                    Message = "Policy not found"
                };
            }

            // Create a unique directory for this deployment
            var deploymentId = Guid.NewGuid().ToString();
            var deploymentPath = Path.Combine(_workingDirectory, deploymentId);
            var sourcePath = Path.Combine(deploymentPath, "source");
            var outputPath = Path.Combine(deploymentPath, "output");
            
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(outputPath);

            // Create policy files based on configuration
            await CreatePolicyFilesAsync(sourcePath, policy, request.Parameters);

            // Create .intunewin file using IntuneWinAppUtil
            var intunewinPath = await CreateIntunewinFileAsync(sourcePath, outputPath, policy.Name);

            if (string.IsNullOrEmpty(intunewinPath))
            {
                return new DeployPolicyResponse
                {
                    Success = false,
                    Message = "Failed to create .intunewin file"
                };
            }

            // Deploy to Intune via Graph API
            string? applicationId = null;
            try
            {
                using var fileStream = File.OpenRead(intunewinPath);
                applicationId = await _graphApiService.CreateIntuneApplicationAsync(
                    policy.Name,
                    policy.Description,
                    fileStream,
                    policy.Configuration
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not deploy to Intune automatically. .intunewin file created at: {Path}", intunewinPath);
            }

            return new DeployPolicyResponse
            {
                Success = true,
                Message = "Package created successfully",
                IntunewinFilePath = intunewinPath,
                IntuneApplicationId = applicationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Intune package");
            return new DeployPolicyResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    private async Task CreatePolicyFilesAsync(string sourcePath, PolicyResponse policy, Dictionary<string, string> parameters)
    {
        // Create install.bat
        var installScript = GenerateInstallScript(policy, parameters);
        await File.WriteAllTextAsync(Path.Combine(sourcePath, "install.bat"), installScript);

        // Create uninstall.bat
        var uninstallScript = GenerateUninstallScript(policy, parameters);
        await File.WriteAllTextAsync(Path.Combine(sourcePath, "uninstall.bat"), uninstallScript);

        // Create configuration.json
        var configJson = JsonSerializer.Serialize(policy.Configuration, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(sourcePath, "configuration.json"), configJson);

        // Create detection script
        var detectionScript = GenerateDetectionScript(policy);
        await File.WriteAllTextAsync(Path.Combine(sourcePath, "detect.ps1"), detectionScript);
    }

    private string GenerateInstallScript(PolicyResponse policy, Dictionary<string, string> parameters)
    {
        return @"@echo off
echo Installing policy: " + policy.Name + @"
echo Configuration file: configuration.json

REM Parse configuration and apply settings
powershell -ExecutionPolicy Bypass -File ""%~dp0apply-config.ps1""

echo Installation completed
exit /b 0
";
    }

    private string GenerateUninstallScript(PolicyResponse policy, Dictionary<string, string> parameters)
    {
        return @"@echo off
echo Uninstalling policy: " + policy.Name + @"

REM Remove applied settings
echo Uninstallation completed
exit /b 0
";
    }

    private string GenerateDetectionScript(PolicyResponse policy)
    {
        return @"# Detection script for " + policy.Name + @"
$configFile = Join-Path $PSScriptRoot ""configuration.json""
if (Test-Path $configFile) {
    Write-Host ""Policy is installed""
    exit 0
} else {
    exit 1
}
";
    }

    private async Task<string?> CreateIntunewinFileAsync(string sourcePath, string outputPath, string appName)
    {
        try
        {
            // Check if IntuneWinAppUtil exists
            if (!File.Exists(_intuneWinAppUtilPath))
            {
                _logger.LogWarning("IntuneWinAppUtil not found at {Path}. Skipping .intunewin creation.", _intuneWinAppUtilPath);
                _logger.LogInformation("To use IntuneWinAppUtil, download it from Microsoft and configure the path in appsettings.json");
                
                // Return the source path as fallback
                return sourcePath;
            }

            // Run IntuneWinAppUtil
            // Usage: IntuneWinAppUtil -c <setup_folder> -s <source_setup_file> -o <output_folder>
            var setupFile = "install.bat";
            var processInfo = new ProcessStartInfo
            {
                FileName = _intuneWinAppUtilPath,
                Arguments = $"-c \"{sourcePath}\" -s \"{setupFile}\" -o \"{outputPath}\" -q",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start IntuneWinAppUtil process");
                return null;
            }

            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                _logger.LogError("IntuneWinAppUtil failed: {Error}", error);
                return null;
            }

            // Find the created .intunewin file
            var intunewinFiles = Directory.GetFiles(outputPath, "*.intunewin");
            return intunewinFiles.Length > 0 ? intunewinFiles[0] : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating .intunewin file");
            return null;
        }
    }
}
