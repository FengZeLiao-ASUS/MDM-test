using IntuneManagement.Data;
using IntuneManagement.Models;
using IntuneManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntuneManagement.Services;

public interface IPolicyService
{
    Task<List<PolicyResponse>> GetPoliciesAsync();
    Task<PolicyResponse?> GetPolicyByIdAsync(int id);
    Task<PolicyResponse> CreatePolicyAsync(PolicyRequest request);
    Task<bool> DeletePolicyAsync(int id);
}

public class PolicyService : IPolicyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(AppDbContext context, ILogger<PolicyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PolicyResponse>> GetPoliciesAsync()
    {
        var policies = await _context.Policies
            .Where(p => p.IsActive)
            .ToListAsync();

        return policies.Select(p => new PolicyResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            PolicyType = p.PolicyType,
            Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(p.Configuration),
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<PolicyResponse?> GetPolicyByIdAsync(int id)
    {
        var policy = await _context.Policies.FindAsync(id);
        
        if (policy == null || !policy.IsActive)
        {
            return null;
        }

        return new PolicyResponse
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            PolicyType = policy.PolicyType,
            Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(policy.Configuration),
            CreatedAt = policy.CreatedAt
        };
    }

    public async Task<PolicyResponse> CreatePolicyAsync(PolicyRequest request)
    {
        var policy = new Policy
        {
            Name = request.Name,
            Description = request.Description,
            PolicyType = request.PolicyType,
            Configuration = JsonSerializer.Serialize(request.Configuration),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        return new PolicyResponse
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            PolicyType = policy.PolicyType,
            Configuration = request.Configuration,
            CreatedAt = policy.CreatedAt
        };
    }

    public async Task<bool> DeletePolicyAsync(int id)
    {
        var policy = await _context.Policies.FindAsync(id);
        
        if (policy == null)
        {
            return false;
        }

        policy.IsActive = false;
        policy.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
