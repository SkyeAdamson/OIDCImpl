using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace OIDCImpl.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health")]
[ApiController]
public class HealthController : ControllerBase
{
	[HttpGet]
	public ActionResult Get()
	{
		return Ok();
	}
}