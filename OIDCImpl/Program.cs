using System.Diagnostics;
using OIDCImpl;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.Configure(opt =>
	opt.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
RegistrationUtil.RegisterServices(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging(opt =>
{
	opt.GetLevel = (context, _, ex) => ex != null || context.Response.StatusCode >= 500 ? LogEventLevel.Error :
		context.Request.Path.StartsWithSegments("/api/v1/health") ? LogEventLevel.Verbose : LogEventLevel.Information;

	opt.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
	{
		diagnosticContext.Set("TraceId", Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier);
	};
});

app.UseExceptionHandler();
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// We add this partial class to allow for integration testing of the Program class.
public partial class Program;