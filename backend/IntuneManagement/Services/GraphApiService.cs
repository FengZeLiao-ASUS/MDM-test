using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using DeviceInfo = IntuneManagement.DTOs.DeviceInfo;

namespace IntuneManagement.Services;

public interface IGraphApiService
{
    Task<DTOs.DeviceListResponse> GetDevicesAsync();
    Task<string> CreateIntuneApplicationAsync(string displayName, string description, Stream intunewinFile);
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

    public async Task<string> CreateIntuneApplicationAsync(string displayName, string description, Stream intunewinFile)
    {
        try
        {
            // Note: This is a simplified version. Actual implementation requires more complex setup
            // including uploading the .intunewin file content and creating the application manifest
            
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
                InstallCommandLine = "install.bat",
                UninstallCommandLine = "uninstall.bat",
                SetupFilePath = "install.bat",
                FileName = "install.intunewin",
                ApplicableArchitectures = WindowsArchitecture.X64,
                MinimumSupportedWindowsRelease = "1607", // Windows 10 version 1607
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
}
