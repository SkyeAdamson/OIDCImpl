namespace OIDCImpl.Config;

public class PasswordOptions
{
	public const string SectionName = "Password";

	public int RequiredLength { get; init; } = 12;

	public bool RequireDigit { get; init; } = true;
	
	public bool RequireLowercase { get; init; } = true;
	
	public bool RequireUppercase { get; init; } = true;
	
	public bool RequireNonAlphanumeric { get; init; } = true;
}