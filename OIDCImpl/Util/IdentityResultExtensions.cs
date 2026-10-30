using Microsoft.AspNetCore.Identity;
using OIDCImpl.Exceptions;

namespace OIDCImpl.Util;

public static class IdentityResultExtensions
{
	private static readonly HashSet<string> DuplicateUserErrorCodes =
		[nameof(IdentityErrorDescriber.DuplicateUserName), nameof(IdentityErrorDescriber.DuplicateEmail)];

	private static readonly HashSet<string> PasswordErrorCodes =
	[
		nameof(IdentityErrorDescriber.PasswordRequiresDigit), nameof(IdentityErrorDescriber.PasswordRequiresLower),
		nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric),
		nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars),
		nameof(IdentityErrorDescriber.PasswordRequiresUpper), nameof(IdentityErrorDescriber.PasswordTooShort)
	];

	public static UserRegistrationErrorCode ToSignUpError(this IdentityResult result)
	{
		if (result.Errors.Any(e => DuplicateUserErrorCodes.Contains(e.Code)))
		{
			return UserRegistrationErrorCode.UserAlreadyExists;
		}

		if (result.Errors.Any(e => PasswordErrorCodes.Contains(e.Code)))
		{
			return UserRegistrationErrorCode.PasswordRequirementsNotMet;
		}

		return UserRegistrationErrorCode.Unknown;
	}
}