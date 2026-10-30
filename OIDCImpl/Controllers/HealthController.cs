using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OIDCImpl.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health")]
[ApiController]
public class HealthController : ControllerBase
{
	[HttpGet]
	[DisableRateLimiting]
	public ActionResult Get()
	{
		return Ok();
	}
}