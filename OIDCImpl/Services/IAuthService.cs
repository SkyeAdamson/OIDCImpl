using OIDCImpl.Requests;

namespace OIDCImpl.Services;

public interface IAuthService
{
	public Task<bool> RegisterAsync(SignUpRequest request, CancellationToken cancellationToken);
}