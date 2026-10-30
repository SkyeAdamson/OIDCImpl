using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OIDCImpl.Requests;
using OIDCImpl.Services;

namespace OIDCImpl.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
	[HttpPost("register")]
	[EnableRateLimiting("registerEndpoint")]
	public async Task<ActionResult> RegisterAsync([FromBody] SignUpRequest request,
		CancellationToken cancellationToken = default)
	{
		await authService.RegisterAsync(request, cancellationToken);
		return Created();
	}
}