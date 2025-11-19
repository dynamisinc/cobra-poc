using Microsoft.AspNetCore.Mvc;

namespace ChecklistAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new 
        { 
            status = "healthy",
            timestamp = DateTime.UtcNow,
            message = "Checklist API is running"
        });
    }
}
