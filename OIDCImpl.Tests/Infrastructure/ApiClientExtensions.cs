using System.Net.Http.Json;
using System.Text.Json;

namespace OIDCImpl.Tests.Infrastructure;

public static class ApiClientExtensions
{
	public const string RegisterPath = "/api/v1/auth/register";
	public const string HealthPath = "/api/v1/health";

	public const string ValidPassword = "ValidPassword123!";

	public static Task<HttpResponseMessage> RegisterAsync(this HttpClient client, string username, string email,
		string password = ValidPassword)
	{
		return client.PostAsJsonAsync(RegisterPath, new { username, email, password });
	}

	public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
	{
		return await response.Content.ReadFromJsonAsync<JsonElement>();
	}
}
