using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OIDCImpl.Requests;
using OIDCImpl.Services;

namespace OIDCImpl.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
	[HttpPost("register")]
	public async Task<ActionResult> RegisterAsync([FromBody] SignUpRequest request,
		CancellationToken cancellationToken = default)
	{
		var result = await authService.RegisterAsync(request, cancellationToken);
		return Ok();
	}
}