using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OIDCImpl.Config;
using OIDCImpl.Data;

namespace OIDCImpl.Tests.Infrastructure;

public class ApiFactory(TestConfig? config = null) : WebApplicationFactory<Program>
{
	private readonly TestConfig _config = config ?? new TestConfig();
	private readonly string _databaseName = Guid.NewGuid().ToString();

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
			new Dictionary<string, string?>
			{
				[$"{RateLimitOptions.SectionName}:{nameof(RateLimitOptions.RegisterRequestsPerHour)}"] =
					_config.RateLimiting.RegisterRequestsPerHour.ToString(),
				[$"{RateLimitOptions.SectionName}:{nameof(RateLimitOptions.GlobalRequestsPerMinute)}"] =
					_config.RateLimiting.GlobalRequestsPerMinute.ToString()
			}));

		builder.ConfigureServices(services =>
		{
			services.RemoveAll<DbContextOptions<AppDbContext>>();
			services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
			services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(_databaseName));
		});
	}
}
