namespace Auth.Application;

/// <summary>
/// Empty sentinel class used to locate the <c>Auth.Application</c> assembly at runtime.
/// Pass <c>typeof(AssemblyMarker)</c> to MediatR's
/// <c>RegisterServicesFromAssemblyContaining</c> and FluentValidation's
/// <c>AddValidatorsFromAssemblyContaining</c> calls in <c>Program.cs</c>.
/// </summary>
public sealed class AssemblyMarker { }