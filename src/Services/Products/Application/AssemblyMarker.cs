namespace Products.Application;

/// <summary>
/// Sentinel type used purely for <see cref="System.Reflection.Assembly"/> discovery.
/// Referenced by MediatR's <c>RegisterServicesFromAssemblyContaining&lt;T&gt;()</c> and
/// FluentValidation's <c>AddValidatorsFromAssemblyContaining&lt;T&gt;()</c> in
/// <c>Program.cs</c> so both libraries can scan the Products.Application assembly
/// for handlers and validators without hard-coding individual type names.
/// </summary>
public sealed class AssemblyMarker { }