using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OIDCImpl.Data;
using OIDCImpl.Models;

namespace OIDCImpl;

public static class RegistrationUtil
{
	public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		RegisterCoreApiServices(services);
		RegisterEntityFramework(services, configuration);
		RegisterIdentityServices(services);
	}

	private static void RegisterCoreApiServices(IServiceCollection services)
	{
		services.AddControllers();
		services.AddApiVersioning(opt =>
		{
			opt.DefaultApiVersion = new ApiVersion(1, 0);
			opt.ReportApiVersions = true;
			opt.ApiVersionReader = new UrlSegmentApiVersionReader();
		}).AddApiExplorer(opt =>
		{
			opt.GroupNameFormat = "'v'VVV";
			opt.SubstituteApiVersionInUrl = true;
		});
		services.AddOpenApi();
	}

	private static void RegisterEntityFramework(IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(opt =>
			opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
	}

	private static void RegisterIdentityServices(IServiceCollection services)
	{
		services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
			.AddEntityFrameworkStores<AppDbContext>()
			.AddDefaultTokenProviders();
	}
}