using FluentValidation;
using LoginRequest = OIDCImpl.Requests.LoginRequest;

namespace OIDCImpl.Validators.Requests;

public class LoginValidator : AbstractValidator<LoginRequest>
{
	public LoginValidator()
	{
		// Identifier may be an email or a username, so further validation needs to be done at EF level.
		RuleFor(x => x.Identifier).NotEmpty();
		RuleFor(x => x.Password).NotEmpty();
	}
}