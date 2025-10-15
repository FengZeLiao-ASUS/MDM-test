using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using DeviceInfo = IntuneManagement.DTOs.DeviceInfo;

namespace IntuneManagement.Services;

public interface IGraphApiService
{
    Task<DTOs.DeviceListResponse> GetDevicesAsync();
    Task<string> CreateIntuneApplicationAsync(string displayName, string description, Stream intunewinFile, Dictionary<string, object>? configuration = null);
}

public class GraphApiService : IGraphApiService
{
    private readonly GraphServiceClient _graphClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GraphApiService> _logger;

    public GraphApiService(IConfiguration configuration, ILogger<GraphApiService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var tenantId = _configuration["AzureAd:TenantId"];
        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];

        var options = new ClientSecretCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, clientSecret, options);

        _graphClient = new GraphServiceClient(clientSecretCredential);
    }

    public async Task<DTOs.DeviceListResponse> GetDevicesAsync()
    {
        try
        {
            var devices = await _graphClient.DeviceManagement.ManagedDevices.GetAsync();
            
            var deviceList = new List<DeviceInfo>();
            
            if (devices?.Value != null)
            {
                foreach (var device in devices.Value)
                {
                    deviceList.Add(new DeviceInfo
                    {
                        Id = device.Id ?? string.Empty,
                        DeviceName = device.DeviceName ?? string.Empty,
                        OperatingSystem = device.OperatingSystem ?? string.Empty,
                        OSVersion = device.OsVersion ?? string.Empty,
                        ComplianceState = device.ComplianceState?.ToString() ?? "Unknown",
                        ManagementAgent = device.ManagementAgent?.ToString() ?? "Unknown",
                        LastSyncDateTime = device.LastSyncDateTime?.DateTime,
                        UserPrincipalName = device.UserPrincipalName ?? string.Empty
                    });
                }
            }

            return new DTOs.DeviceListResponse
            {
                Devices = deviceList,
                TotalCount = deviceList.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching devices from Microsoft Graph API");
            throw;
        }
    }

    public async Task<string> CreateIntuneApplicationAsync(string displayName, string description, Stream intunewinFile, Dictionary<string, object>? configuration = null)
    {
        try
        {
            // Note: This is a simplified version. Actual implementation requires more complex setup
            // including uploading the .intunewin file content and creating the application manifest
            
            // Extract values from configuration if available
            string installCommand = "install.bat";
            string uninstallCommand = "uninstall.bat";
            string architecture = "x64";
            string minOSVersion = "1607";
            
            if (configuration != null)
            {
                if (configuration.TryGetValue("installCommand", out var installCmd))
                    installCommand = installCmd?.ToString() ?? "install.bat";
                    
                if (configuration.TryGetValue("uninstallCommand", out var uninstallCmd))
                    uninstallCommand = uninstallCmd?.ToString() ?? "uninstall.bat";
                    
                if (configuration.TryGetValue("architecture", out var arch))
                    architecture = arch?.ToString()?.ToLower() ?? "x64";
                    
                if (configuration.TryGetValue("minOSVersion", out var minOS))
                {
                    var osVersion = minOS?.ToString() ?? "10.0.0.0";
                    // Convert version like "10.0.14393.0" to Windows release "1607"
                    minOSVersion = ConvertOSVersionToRelease(osVersion);
                }
            }
            
            var applicableArchitectures = architecture.ToLower() switch
            {
                "x86" => WindowsArchitecture.X86,
                "x64" => WindowsArchitecture.X64,
                "arm" => WindowsArchitecture.Arm,
                _ => WindowsArchitecture.X64
            };
            
            var application = new Win32LobApp
            {
                DisplayName = displayName,
                Description = description,
                Publisher = "Custom Publisher",
                IsFeatured = false,
                PrivacyInformationUrl = null,
                InformationUrl = null,
                Owner = null,
                Developer = null,
                Notes = "Created via API",
                // Required fields for Win32LobApp
                InstallCommandLine = installCommand,
                UninstallCommandLine = uninstallCommand,
                SetupFilePath = "install.bat",
                FileName = "install.intunewin",
                ApplicableArchitectures = applicableArchitectures,
                MinimumSupportedWindowsRelease = minOSVersion,
                InstallExperience = new Win32LobAppInstallExperience
                {
                    RunAsAccount = RunAsAccountType.System,
                    DeviceRestartBehavior = Win32LobAppRestartBehavior.BasedOnReturnCode
                }
            };

            var result = await _graphClient.DeviceAppManagement.MobileApps.PostAsync(application);
            
            return result?.Id ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Intune application");
            throw;
        }
    }
    
    private string ConvertOSVersionToRelease(string osVersion)
    {
        // Convert Windows version to release number
        // Example: "10.0.14393.0" -> "1607" (Anniversary Update)
        if (osVersion.StartsWith("10.0."))
        {
            var buildNumber = osVersion.Split('.')[2];
            return buildNumber switch
            {
                "10240" => "1507",
                "10586" => "1511",
                "14393" => "1607",
                "15063" => "1703",
                "16299" => "1709",
                "17134" => "1803",
                "17763" => "1809",
                "18362" => "1903",
                "18363" => "1909",
                "19041" => "2004",
                "19042" => "20H2",
                "19043" => "21H1",
                "19044" => "21H2",
                "22000" => "21H2", // Windows 11
                _ => "1607" // Default to Anniversary Update
            };
        }
        return "1607"; // Default to Anniversary Update
    }
}
