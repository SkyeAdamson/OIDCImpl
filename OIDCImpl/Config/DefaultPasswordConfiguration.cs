namespace OIDCImpl.Config;

public class DefaultPasswordConfiguration : IPasswordConfiguration
{
	// This goes against the current NIST guidelines for password-only authentication, which puts the focus
	// on length over mixed character types.
	// MFA and passkeys should be supported during this project, so I'm opting for a middle ground.

	public int RequiredLength => 12;

	public bool RequireDigit => true;

	public bool RequireLowercase => true;

	public bool RequireUppercase => true;

	public bool RequireNonAlphanumeric => true;
}