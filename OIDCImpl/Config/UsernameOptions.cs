namespace OIDCImpl.Config;

public class UsernameOptions
{
	public const string SectionName = "Username";
	
	public int MaxLength { get; init; } = 32;
}