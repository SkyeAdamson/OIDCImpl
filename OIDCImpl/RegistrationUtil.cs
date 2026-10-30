using System.Diagnostics;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OIDCImpl.Config;
using OIDCImpl.Data;
using OIDCImpl.Exceptions.Handlers;
using OIDCImpl.Models;
using OIDCImpl.Services;
using OIDCImpl.Validators;

namespace OIDCImpl;

public static class RegistrationUtil
{
	public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		RegisterCoreApiServices(services);
		RegisterEntityFramework(services, configuration);
		RegisterIdentityServices(services);
		RegisterAuthServices(services);
		RegisterValidators(services);
	}

	private static void RegisterCoreApiServices(IServiceCollection services)
	{
		services.AddProblemDetails(opt => opt.CustomizeProblemDetails = context =>
		{
			context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
		});
		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddControllers(opt => opt.Filters.Add<ValidationFilter>());
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
		services.AddSingleton<IPasswordConfiguration, DefaultPasswordConfiguration>();
		services.AddSingleton<IUsernameConfiguration, DefaultUsernameConfiguration>();
		
		services.AddOptions<IdentityOptions>()
			.Configure<IPasswordConfiguration>((opt, passwordConfig) =>
			{
				opt.Password.RequiredLength = passwordConfig.RequiredLength;
				opt.Password.RequireDigit = passwordConfig.RequireDigit;
				opt.Password.RequireLowercase = passwordConfig.RequireLowercase;
				opt.Password.RequireNonAlphanumeric = passwordConfig.RequireNonAlphanumeric;
				opt.Password.RequireUppercase = passwordConfig.RequireUppercase;

				opt.User.RequireUniqueEmail = true;
			});

		services.AddIdentity<ApplicationUser, IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>()
			.AddDefaultTokenProviders();
	}

	private static void RegisterAuthServices(IServiceCollection services)
	{
		services.AddScoped<IAuthService, AuthService>();
	}

	private static void RegisterValidators(IServiceCollection services)
	{
		services.AddValidatorsFromAssemblyContaining(typeof(RegistrationUtil));
	}
}