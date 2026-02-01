using Microsoft.AspNetCore.Mvc;

namespace Paperthin;

public static class ValidationHandler
{
	public static void UseApiValidation(this IServiceCollection services)
	{
		services.Configure<ApiBehaviorOptions>(options =>
			options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory);
	}

	private static IActionResult InvalidModelStateResponseFactory(ActionContext context)
	{
		Dictionary<string, string[]> errors = context.ModelState
			.Where(state => state.Value?.Errors.Count > 0)
			.ToDictionary(
				pair => pair.Key,
				pair => pair.Value!.Errors
					.Select(error => error.ErrorMessage)
					.ToArray());

		return new BadRequestObjectResult(new
		{
			message = "Validation failed.",
			errors,
		});
	}
}
