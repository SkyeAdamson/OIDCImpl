using FluentValidation;
using Microsoft.Extensions.Options;
using OIDCImpl.Config;
using OIDCImpl.Requests;

namespace OIDCImpl.Validators.Requests;

public class SignUpValidator : AbstractValidator<SignUpRequest>
{
	public SignUpValidator(IOptions<UsernameOptions> usernameOptions, IOptions<PasswordOptions> passwordOptions)
	{
		RuleFor(x => x.Username).NotEmpty().MaximumLength(usernameOptions.Value.MaxLength);
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Password).NotEmpty().MinimumLength(passwordOptions.Value.RequiredLength);
	}
}