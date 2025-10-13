namespace IntuneManagement.DTOs;

public class DeviceInfo
{
    public string Id { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string ComplianceState { get; set; } = string.Empty;
    public string ManagementAgent { get; set; } = string.Empty;
    public DateTime? LastSyncDateTime { get; set; }
    public string UserPrincipalName { get; set; } = string.Empty;
}

public class DeviceListResponse
{
    public List<DeviceInfo> Devices { get; set; } = new();
    public int TotalCount { get; set; }
}
