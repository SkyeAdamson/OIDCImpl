using System.Net;
using OIDCImpl.Tests.Infrastructure;

namespace OIDCImpl.Tests;

[TestFixture]
public class RateLimitingTests
{
	private const int Limit = 2;

	private ApiFactory _factory = null!;
	private HttpClient _client = null!;

	[SetUp]
	public void SetUp()
	{
		_factory = new ApiFactory(new TestConfig
		{
			RateLimiting = new TestRateLimitingConfig { RegisterRequestsPerHour = Limit }
		});
		_client = _factory.CreateClient();
	}

	[TearDown]
	public void TearDown()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[Test]
	public async Task Register_WithinLimit_IsNotThrottled()
	{
		for (var i = 0; i < Limit; i++)
		{
			var response = await _client.RegisterAsync($"user{i}", $"user{i}@example.com");
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
		}
	}

	[Test]
	public async Task Register_ExceedingLimit_Returns429_WithProblemDetails()
	{
		for (var i = 0; i < Limit; i++)
		{
			await _client.RegisterAsync($"user{i}", $"user{i}@example.com");
		}

		var response = await _client.RegisterAsync("overlimit", "overlimit@example.com");
		var body = await response.ReadJsonAsync();

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
		Assert.Multiple(() =>
		{
			Assert.That(response.Content.Headers.ContentType?.MediaType, Does.Contain("json"));
			Assert.That(body.GetProperty("status").GetInt32(), Is.EqualTo(429));
			Assert.That(body.GetProperty("traceId").GetString(), Is.Not.Empty);
		});
	}

	[Test]
	public async Task RegisterLimit_DoesNotThrottleOtherEndpoints()
	{
		for (var i = 0; i <= Limit; i++)
		{
			await _client.RegisterAsync($"user{i}", $"user{i}@example.com");
		}

		var health = await _client.GetAsync(ApiClientExtensions.HealthPath);

		Assert.That(health.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}
}
