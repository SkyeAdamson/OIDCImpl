namespace OIDCImpl.Config;

public interface IPasswordConfiguration
{
	int RequiredLength { get; }

	bool RequireDigit { get; }
	
	bool RequireLowercase { get; }
	
	bool RequireUppercase { get; }
	
	bool RequireNonAlphanumeric { get; }
}