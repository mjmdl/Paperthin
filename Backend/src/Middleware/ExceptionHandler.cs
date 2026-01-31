using Microsoft.AspNetCore.Diagnostics;

namespace Paperthin;

public static class ExceptionHandler
{
	public static void UseApiExceptionHandler(this IApplicationBuilder app)
	{
		app.UseExceptionHandler(appBuilder => appBuilder.Run(HandleApiException));
	}

	private static async Task HandleApiException(HttpContext context)
	{
		IExceptionHandlerFeature? feature = context.Features.Get<IExceptionHandlerFeature>();
		if (feature == null)
			return;

		context.Response.ContentType = "application/json";

		Exception error = feature.Error;

		context.Response.StatusCode = error switch
		{
			ArgumentNullException => StatusCodes.Status400BadRequest,
			ArgumentException => StatusCodes.Status400BadRequest,
			UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
			NotImplementedException => StatusCodes.Status501NotImplemented,
			InvalidOperationException => StatusCodes.Status409Conflict,
			_ => StatusCodes.Status500InternalServerError,
		};

		await context.Response.WriteAsJsonAsync(new { error = error.Message });
	}
}