using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OIDCImpl.Models;
using OIDCImpl.Tests.Infrastructure;

namespace OIDCImpl.Tests;

[TestFixture]
public class SignUpEndpointTests
{
	private ApiFactory _factory = null!;
	private HttpClient _client = null!;

	[SetUp]
	public void SetUp()
	{
		_factory = new ApiFactory();
		_client = _factory.CreateClient();
	}

	[TearDown]
	public void TearDown()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[Test]
	public async Task Register_WithValidRequest_Returns201_AndPersistsTrimmedUser()
	{
		var response = await _client.RegisterAsync("  newuser  ", "  new@example.com  ");

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

		using var scope = _factory.Services.CreateScope();
		var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
			.FindByNameAsync("newuser");
		Assert.That(user, Is.Not.Null);
		Assert.That(user!.Email, Is.EqualTo("new@example.com"));
	}

	[TestCase("existing", "other@example.com", TestName = "Register_DuplicateUsername_Returns409")]
	[TestCase("someoneelse", "existing@example.com", TestName = "Register_DuplicateEmail_Returns409")]
	public async Task Register_WithExistingAccount_Returns409_WithErrorCode(string username, string email)
	{
		await _client.RegisterAsync("existing", "existing@example.com");

		var response = await _client.RegisterAsync(username, email);
		var body = await response.ReadJsonAsync();

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
		Assert.Multiple(() =>
		{
			Assert.That(body.GetProperty("errorCode").GetString(), Is.EqualTo("UserAlreadyExists"));
			Assert.That(body.GetProperty("traceId").GetString(), Is.Not.Empty);
		});
	}

	[Test]
	public async Task Register_WithPasswordFailingIdentityRules_Returns400_WithErrorCode()
	{
		// Long enough to pass request validation but has no uppercase character, so Identity rejects it.
		var response = await _client.RegisterAsync("newuser", "new@example.com", "nouppercase123!!");
		var body = await response.ReadJsonAsync();

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
		Assert.That(body.GetProperty("errorCode").GetString(), Is.EqualTo("PasswordRequirementsNotMet"));
	}

	[Test]
	public async Task Register_WithInvalidFields_Returns400_ListingEachInvalidField_AndCreatesNoUser()
	{
		var response = await _client.RegisterAsync(new string('a', 33), "not-an-email", "short");
		var errors = (await response.ReadJsonAsync()).GetProperty("errors");

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
		Assert.Multiple(() =>
		{
			Assert.That(errors.TryGetProperty("Username", out _), Is.True);
			Assert.That(errors.TryGetProperty("Email", out _), Is.True);
			Assert.That(errors.TryGetProperty("Password", out _), Is.True);
		});

		using var scope = _factory.Services.CreateScope();
		var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().Users;
		Assert.That(users, Is.Empty);
	}

	[Test]
	public async Task Register_WithMissingBodyFields_Returns400_NotAServerError()
	{
		var response = await _client.PostAsync(ApiClientExtensions.RegisterPath,
			new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
	}
}
