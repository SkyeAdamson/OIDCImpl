namespace OIDCImpl.Tests.Infrastructure;

/// <summary>Per-test overrides applied on top of the API's configuration.</summary>
public class TestConfig
{
	public TestRateLimitingConfig RateLimiting { get; init; } = new();
}

public class TestRateLimitingConfig
{
	// Defaults are high enough that tests not about rate limiting are never throttled.
	public int RegisterRequestsPerHour { get; init; } = 1000;

	public int GlobalRequestsPerMinute { get; init; } = 1000;
}
