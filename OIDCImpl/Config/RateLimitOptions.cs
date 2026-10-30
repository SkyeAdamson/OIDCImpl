namespace OIDCImpl.Config;

public class RateLimitOptions
{
	public const string SectionName = "RateLimiting";
	
	public int RegisterRequestsPerHour { get; init; } = 5;
	
	public int GlobalRequestsPerMinute { get; init; } = 100;
}