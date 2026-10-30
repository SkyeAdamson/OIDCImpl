using OIDCImpl.Requests;

namespace OIDCImpl.Services;

public interface IAuthService
{
	public Task RegisterAsync(SignUpRequest request, CancellationToken cancellationToken);
}