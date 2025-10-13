namespace IntuneManagement.DTOs;

public class PolicyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class PolicyResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public Dictionary<string, object>? Configuration { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeployPolicyRequest
{
    public int PolicyId { get; set; }
    public string TargetGroup { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
}

public class DeployPolicyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? IntuneApplicationId { get; set; }
    public string? IntunewinFilePath { get; set; }
}
