using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OIDCImpl.Validators;

public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		foreach (var arg in context.ActionArguments.Values.Where(a => a is not null))
		{
			var validator =
				serviceProvider.GetService(typeof(IValidator<>).MakeGenericType(arg!.GetType())) as IValidator;
			if (validator is null)
			{
				continue;
			}

			var result =
				await validator.ValidateAsync(new ValidationContext<object>(arg), context.HttpContext.RequestAborted);
			if (result.IsValid)
			{
				continue;
			}

			context.Result = new BadRequestObjectResult(new ValidationProblemDetails(result.ToDictionary()));
			return;
		}

		await next();
	}
}