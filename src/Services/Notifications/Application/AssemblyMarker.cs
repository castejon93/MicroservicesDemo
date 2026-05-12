namespace Notifications.Application;

/// <summary>
/// Empty marker type used to reference this assembly in dependency injection
/// registration calls such as:
/// <code>
///   cfg.RegisterServicesFromAssemblyContaining&lt;AssemblyMarker&gt;()
/// </code>
/// This avoids magic strings or typeof(SomeRandomClass) references scattered
/// around Program.cs.
/// </summary>
public sealed class AssemblyMarker;
