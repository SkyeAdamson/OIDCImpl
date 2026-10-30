using Microsoft.AspNetCore.Identity;
using OIDCImpl.Exceptions;
using OIDCImpl.Models;
using OIDCImpl.Requests;
using OIDCImpl.Util;

namespace OIDCImpl.Services;

public class AuthService(UserManager<ApplicationUser> userManager) : IAuthService
{
	public async Task<bool> RegisterAsync(SignUpRequest request, CancellationToken cancellationToken)
	{
		var trimmedUsername = request.Username.Trim();
		var trimmedEmail = request.Email.Trim();
		ValidateRequestFields(trimmedUsername, trimmedEmail, request.Password);

		var user = new ApplicationUser
		{
			UserName = trimmedUsername,
			Email = trimmedEmail
		};
		
		var result = await userManager.CreateAsync(user, request.Password);
		if (!result.Succeeded)
		{
			IdentityErrorHandler.HandleIdentityError(result);
		}

		return true;
	}

	private static void ValidateRequestFields(string trimmedUsername, string trimmedEmail, string password)
	{
		if (string.IsNullOrWhiteSpace(trimmedUsername))
		{
			throw new UserRegistrationException(UserRegistrationErrorCode.UsernameEmpty);
		}

		if (string.IsNullOrWhiteSpace(trimmedEmail))
		{
			throw new UserRegistrationException(UserRegistrationErrorCode.EmailEmpty);
		}

		if (string.IsNullOrWhiteSpace(password))
		{
			throw new UserRegistrationException(UserRegistrationErrorCode.PasswordEmpty);
		}
	}
}