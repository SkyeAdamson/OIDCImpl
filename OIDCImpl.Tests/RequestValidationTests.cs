using Microsoft.Extensions.Options;
using OIDCImpl.Config;
using OIDCImpl.Requests;
using OIDCImpl.Validators.Requests;

namespace OIDCImpl.Tests;

[TestFixture]
public class RequestValidationTests
{
	private readonly IOptions<UsernameOptions> _usernameConfig = Options.Create(new UsernameOptions
	{
		MaxLength = 32
	});

	private readonly IOptions<PasswordOptions> _passwordConfig = Options.Create(new PasswordOptions
	{
		RequiredLength = 12,
		RequireDigit = true,
		RequireLowercase = true,
		RequireUppercase = true,
		RequireNonAlphanumeric = true
	});

	private const string ValidUsername = "ValidUsername123";
	private const string ValidPassword = "ValidPassword123!";
	private const string ValidEmail = "valid.email@xyz.com";

	[Test]
	public void SignUp_Fails_WhenUsername_Is_Too_Long()
	{
		var validator = new SignUpValidator(_usernameConfig, _passwordConfig);
		var request = new SignUpRequest
		{
			Username = new string('a', 51),
			Password = ValidPassword,
			Email = ValidEmail
		};
		
		var result = validator.Validate(request);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });

        Assert.That(result.Errors[0].ErrorMessage, Does.Contain($"The length of 'Username' must be {_usernameConfig.Value.MaxLength} characters or fewer"));
	}

	[Test]
	public void SignUp_Fails_WhenPassword_Is_Too_Short()
	{
		var validator = new SignUpValidator(_usernameConfig, _passwordConfig);
		var request = new SignUpRequest
		{
			Username = ValidUsername,
			Password = "Short1!",
			Email = ValidEmail
		};
		
		var result = validator.Validate(request);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
        });
        
        Assert.That(result.Errors[0].ErrorMessage, Does.Contain($"The length of 'Password' must be at least {_passwordConfig.Value.RequiredLength} characters"));
	}

	[Test]
	public void SignUp_Fails_WhenEmail_Is_Invalid()
	{
		var validator = new SignUpValidator(_usernameConfig, _passwordConfig);
		var request = new SignUpRequest
		{
			Username = ValidUsername,
			Password = ValidPassword,
			Email = "invalid.email"
		};
		
		var result = validator.Validate(request);
		Assert.Multiple(() =>
		{
			Assert.That(result.IsValid, Is.False);
			Assert.That(result.Errors, Has.Count.EqualTo(1));
		});
        
		Assert.That(result.Errors[0].ErrorMessage, Does.Contain("'Email' is not a valid email address."));
	}

	[Test]
	public void Login_Fails_WhenIdentifier_Is_Empty()
	{
		var validator = new LoginValidator();
		var request = new LoginRequest
		{
			Identifier = string.Empty,
			Password = ValidPassword,
		};
		
		var result = validator.Validate(request);
		Assert.Multiple(() =>
		{
			Assert.That(result.IsValid, Is.False);
			Assert.That(result.Errors, Has.Count.EqualTo(1));
		});
        
		Assert.That(result.Errors[0].ErrorMessage, Does.Contain("'Identifier' must not be empty."));
	}
	
	[Test]
	public void Login_Fails_WhenPassword_Is_Empty()
	{
		var validator = new LoginValidator();
		var request = new LoginRequest
		{
			Identifier = ValidUsername,
			Password = string.Empty,
		};
		
		var result = validator.Validate(request);
		Assert.Multiple(() =>
		{
			Assert.That(result.IsValid, Is.False);
			Assert.That(result.Errors, Has.Count.EqualTo(1));
		});
        
		Assert.That(result.Errors[0].ErrorMessage, Does.Contain("'Password' must not be empty."));
	}

	[Test]
	public void SignUp_Succeeds_When_All_Fields_Are_Valid()
	{
		var validator = new SignUpValidator(_usernameConfig, _passwordConfig);
		var request = new SignUpRequest
		{
			Username = ValidUsername,
			Password = ValidPassword,
			Email = ValidEmail
		};
		
		var result = validator.Validate(request);
		Assert.That(result.IsValid, Is.True);
	}

	[Test]
	public void Login_Succeeds_When_All_Fields_Are_Valid()
	{
		var validator = new LoginValidator();
		 var request = new LoginRequest
		 {
			 Identifier = ValidUsername,
			 Password = ValidPassword,
		 };
		 
		 var result = validator.Validate(request);
		 Assert.That(result.IsValid, Is.True);
	}
}