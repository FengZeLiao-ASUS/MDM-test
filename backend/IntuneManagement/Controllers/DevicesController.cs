using Microsoft.AspNetCore.Mvc;
using IntuneManagement.Services;

namespace IntuneManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IGraphApiService _graphApiService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(IGraphApiService graphApiService, ILogger<DevicesController> logger)
    {
        _graphApiService = graphApiService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDevices()
    {
        try
        {
            var devices = await _graphApiService.GetDevicesAsync();
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching devices");
            return StatusCode(500, new { message = "Error fetching devices", error = ex.Message });
        }
    }
}
