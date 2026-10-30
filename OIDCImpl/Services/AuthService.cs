using Microsoft.AspNetCore.Identity;
using OIDCImpl.Exceptions;
using OIDCImpl.Models;
using OIDCImpl.Requests;
using OIDCImpl.Util;

namespace OIDCImpl.Services;

public class AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger) : IAuthService
{
	public async Task RegisterAsync(SignUpRequest request, CancellationToken cancellationToken)
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
		if (result.Succeeded)
		{
			return;
		}

		var errorCode = result.ToSignUpError();
		logger.Log(errorCode == UserRegistrationErrorCode.Unknown ? LogLevel.Error : LogLevel.Information, "Registration failed with {ErrorCode}; identity codes: {IdentityCodes}", errorCode, result.Errors.Select(e => e.Code));
		throw new UserRegistrationException(errorCode);
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