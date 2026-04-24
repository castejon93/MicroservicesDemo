/// <summary>
/// Global middleware that catches <see cref="FluentValidation.ValidationException"/> instances
/// thrown by the MediatR <c>ValidationBehavior</c> and translates them into a consistent
/// HTTP 400 response. This keeps handlers free of transport concerns and gives the Gateway
/// (and clients) a single, predictable error contract for all validation failures.
/// </summary>
public sealed class ValidationExceptionMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invokes the next component in the pipeline and, on validation failure, writes a
    /// JSON payload of the shape <c>{ title, errors: { property: [messages] } }</c> with
    /// status code <c>400 Bad Request</c>. Non-validation exceptions are intentionally
    /// allowed to propagate so the framework's default handling applies.
    /// </summary>
    /// <param name="ctx">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (FluentValidation.ValidationException ex)
        {
            // Map validation errors into a property-keyed dictionary so clients can
            // surface field-level messages without parsing free-form text.
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