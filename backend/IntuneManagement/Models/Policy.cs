namespace IntuneManagement.Models;

public class Policy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty; // JSON string
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
