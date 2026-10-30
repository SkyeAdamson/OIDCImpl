using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OIDCImpl.Exceptions.Handlers;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		var (status, title, detail, code) = exception switch
		{
			OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested => (499,
				"Request cancelled.", null, null),
			BadHttpRequestException ex => (ex.StatusCode, "Bad request.", null, null),
			UserRegistrationException { ErrorCode: UserRegistrationErrorCode.Unknown } => (500,
				"An unexpected error occurred.", "Please try again later.", "Unknown"),
			UserRegistrationException ex => (ex.ErrorCode == UserRegistrationErrorCode.UserAlreadyExists ? 409 : 400,
				"Registration failed.", ex.Message, ex.ErrorCode.ToString()),
			_ => (500, "An unexpected error occurred.", "Please try again later.", null)
		};

		if (status >= 500)
		{
			logger.LogError(exception, "Unhandled exception");
		}
		else
		{
			logger.LogInformation("Request failed: {Status} {Code}", status, code);
		}

		httpContext.Response.StatusCode = status;
		var problem = new ProblemDetails
		{
			Status = status,
			Title = title,
			Detail = detail,
			Instance = httpContext.Request.Path
		};
		if (code is not null)
		{
			problem.Extensions["errorCode"] = code;
		}

		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			ProblemDetails = problem,
			Exception = exception
		});
	}
}