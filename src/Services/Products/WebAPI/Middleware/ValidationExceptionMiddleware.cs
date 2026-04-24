/// <summary>
/// Global middleware that catches <see cref="FluentValidation.ValidationException"/>
/// instances thrown by the MediatR <c>ValidationBehavior</c> and translates them
/// into a consistent HTTP 400 response. Keeps handlers free of transport concerns
/// and gives clients a single predictable error contract for validation failures.
/// </summary>
public sealed class ValidationExceptionMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invokes the next component in the pipeline and, on validation failure, writes
    /// a JSON payload of the shape <c>{ title, errors: { property: [messages] } }</c>
    /// with status code <c>400 Bad Request</c>. Non-validation exceptions propagate
    /// to the framework's default handling.
    /// </summary>
    /// <param name="ctx">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (FluentValidation.ValidationException ex)
        {
            // Group failures by property so the client can render field-level messages.
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(new
            {
                title = "Validation failed",
                errors = ex.Errors.GroupBy(e => e.PropertyName)
                                  .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }
    }
}