using Microsoft.AspNetCore.Identity;
using OIDCImpl.Exceptions;

namespace OIDCImpl.Util;

public static class IdentityErrorHandler
{
	private static readonly string[] PasswordErrorCodes =
	[
		"PasswordRequiresDigit",
		"PasswordRequiresLower",
		"PasswordRequiresNonAlphanumeric",
		"PasswordRequiresUniqueChars",
		"PasswordRequiresUpper",
		"PasswordTooShort"
	];

	private static readonly string[] UsernameErrorCodes =
	[
		"DuplicateUserName",
		"DuplicateEmail"
	];
	
	public static void HandleIdentityError(IdentityResult result)
	{
		if (result.Succeeded)
		{
			return;
		}

		var userDuplicateErrors = result.Errors.Where(x => UsernameErrorCodes.Contains(x.Code));
		if (userDuplicateErrors.Any())
		{
			throw new UserRegistrationException(UserRegistrationErrorCode.UserAlreadyExists);
		}

		var passwordErrors = result.Errors.Where(x => PasswordErrorCodes.Contains(x.Code));
		if (passwordErrors.Any())
		{
			throw new UserRegistrationException(UserRegistrationErrorCode.PasswordRequirementsNotMet);
		}
		
		// Anything we're not explicitly handling, map to a generic error code
		throw new UserRegistrationException(UserRegistrationErrorCode.Unknown);
	}
}