using Microsoft.AspNetCore.Mvc;
using IntuneManagement.DTOs;
using IntuneManagement.Services;

namespace IntuneManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;
    private readonly IIntunePackageService _intunePackageService;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(
        IPolicyService policyService,
        IIntunePackageService intunePackageService,
        ILogger<PoliciesController> logger)
    {
        _policyService = policyService;
        _intunePackageService = intunePackageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<PolicyResponse>>> GetPolicies()
    {
        var policies = await _policyService.GetPoliciesAsync();
        return Ok(policies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyResponse>> GetPolicy(int id)
    {
        var policy = await _policyService.GetPolicyByIdAsync(id);
        
        if (policy == null)
        {
            return NotFound(new { message = "Policy not found" });
        }

        return Ok(policy);
    }

    [HttpPost]
    public async Task<ActionResult<PolicyResponse>> CreatePolicy([FromBody] PolicyRequest request)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            return BadRequest(new { message = "Policy name is required" });
        }

        var policy = await _policyService.CreatePolicyAsync(request);
        return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePolicy(int id)
    {
        var success = await _policyService.DeletePolicyAsync(id);
        
        if (!success)
        {
            return NotFound(new { message = "Policy not found" });
        }

        return NoContent();
    }

    [HttpPost("deploy")]
    public async Task<ActionResult<DeployPolicyResponse>> DeployPolicy([FromBody] DeployPolicyRequest request)
    {
        if (request.PolicyId <= 0)
        {
            return BadRequest(new { message = "Invalid policy ID" });
        }

        var response = await _intunePackageService.CreateIntunePackageAsync(request);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
