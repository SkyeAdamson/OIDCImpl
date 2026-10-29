using FluentValidation;
using OIDCImpl.Config;
using OIDCImpl.Requests;

namespace OIDCImpl.Validators.Requests;

public class SignUpValidator : AbstractValidator<SignUpRequest>
{
	public SignUpValidator(IUsernameConfiguration usernameConfiguration, IPasswordConfiguration passwordConfiguration)
	{
		RuleFor(x => x.Username).NotEmpty().MaximumLength(usernameConfiguration.MaxLength);
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Password).NotEmpty().MinimumLength(passwordConfiguration.RequiredLength);
	}
}