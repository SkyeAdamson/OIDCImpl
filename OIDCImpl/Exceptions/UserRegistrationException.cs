namespace OIDCImpl.Exceptions;

public class UserRegistrationException : Exception
{
	public UserRegistrationException(UserRegistrationErrorCode errorCode) : base(ToExceptionMessage(errorCode))
	{
		ErrorCode = errorCode;
	}

	public UserRegistrationException(UserRegistrationErrorCode errorCode, Exception inner) : base(ToExceptionMessage(errorCode), inner)
	{
		ErrorCode = errorCode;
	}

	public UserRegistrationErrorCode ErrorCode { get; private set; }

	private static string ToExceptionMessage(UserRegistrationErrorCode errorCode)
	{
		return errorCode switch
		{
			UserRegistrationErrorCode.UserAlreadyExists => "Username or email already exists.",
			UserRegistrationErrorCode.PasswordRequirementsNotMet => "Password does not meet requirements.",
			_ => "Registration failed."
		};
	}
}

public enum UserRegistrationErrorCode
{
	Unknown = 0,
	UsernameEmpty,
	EmailEmpty,
	PasswordEmpty,
	UserAlreadyExists,
	PasswordRequirementsNotMet
}