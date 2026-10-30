using System.Diagnostics;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OIDCImpl.Config;
using OIDCImpl.Data;
using OIDCImpl.Exceptions.Handlers;
using OIDCImpl.Models;
using OIDCImpl.Services;
using OIDCImpl.Validators;
using PasswordOptions = OIDCImpl.Config.PasswordOptions;

namespace OIDCImpl;

public static class RegistrationUtil
{
	public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		RegisterOptions(services);
		RegisterLogging(services);
		RegisterCoreApiServices(services);
		RegisterEntityFramework(services, configuration);
		RegisterIdentityServices(services);
		RegisterAuthServices(services);
		RegisterValidators(services);
		RegisterRateLimiting(services, configuration);
	}

	private static void RegisterOptions(IServiceCollection services)
	{
		services.AddOptions<PasswordOptions>().BindConfiguration(PasswordOptions.SectionName).ValidateDataAnnotations()
			.ValidateOnStart();
		services.AddOptions<UsernameOptions>().BindConfiguration(UsernameOptions.SectionName).ValidateDataAnnotations();
		services.AddOptions<RateLimitOptions>().BindConfiguration(RateLimitOptions.SectionName).ValidateDataAnnotations();
	}

	private static void RegisterLogging(IServiceCollection services)
	{
		services.Configure<ApiBehaviorOptions>(opt =>
		{
			var defaultFactory = opt.InvalidModelStateResponseFactory;
			opt.InvalidModelStateResponseFactory = context =>
			{
				var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OIDCImpl.Validation");
				logger.LogInformation("Model validation failed for {Fields}",
					context.ModelState.Where(e => e.Value?.Errors.Count > 0).Select(e => e.Key));
				return defaultFactory(context);
			};
		});
	}

	private static void RegisterCoreApiServices(IServiceCollection services)
	{
		services.AddProblemDetails(opt => opt.CustomizeProblemDetails = context =>
		{
			context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
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
		services.AddOptions<IdentityOptions>()
			.Configure<IOptions<PasswordOptions>>((opt, passwordConfig) =>
			{
				var configVal = passwordConfig.Value;

				opt.Password.RequiredLength = configVal.RequiredLength;
				opt.Password.RequireDigit = configVal.RequireDigit;
				opt.Password.RequireLowercase = configVal.RequireLowercase;
				opt.Password.RequireNonAlphanumeric = configVal.RequireNonAlphanumeric;
				opt.Password.RequireUppercase = configVal.RequireUppercase;

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

	private static void RegisterRateLimiting(IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<RateLimiterOptions>().Configure<IOptions<RateLimitOptions>>((limiter, limits) =>
		{
			var opt = limits.Value;
			limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
			limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
				RateLimitPartition.GetSlidingWindowLimiter(ClientKey(context), _ => new SlidingWindowRateLimiterOptions
				{
					PermitLimit = opt.GlobalRequestsPerMinute,
					Window = TimeSpan.FromMinutes(1),
					SegmentsPerWindow = 6,
					QueueLimit = 0
				}));

			limiter.AddPolicy("registerEndpoint", context => RateLimitPartition.GetSlidingWindowLimiter(
				ClientKey(context), _ => new SlidingWindowRateLimiterOptions
				{
					PermitLimit = opt.RegisterRequestsPerHour,
					Window = TimeSpan.FromHours(1),
					SegmentsPerWindow = 6,
					QueueLimit = 0
				}));

			limiter.OnRejected = async (context, cancellationToken) =>
			{
				var httpContext = context.HttpContext;
				if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
				{
					httpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
				}

				httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OIDCImpl.RateLimit")
					.LogWarning("Rate limit exceeded for {Path} by {ClientKey}", httpContext.Request.Path,
						ClientKey(httpContext));

				var problems = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
				await problems.TryWriteAsync(new ProblemDetailsContext
				{
					HttpContext = httpContext,
					ProblemDetails = new ProblemDetails
					{
						Status = 429,
						Title = "Too many requests.",
						Detail = "Please try again later.",
					}
				});
			};
		});
		services.AddRateLimiter();
	}

	private static string ClientKey(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}