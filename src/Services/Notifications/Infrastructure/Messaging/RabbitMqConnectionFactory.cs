using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Notifications.Infrastructure.Messaging;

/// <summary>
/// Creates and returns a singleton <see cref="IConnection"/> to RabbitMQ.
///
/// <para>
/// The connection is shared across all consumer background services in this service.
/// Each consumer opens its own <see cref="IChannel"/> from this shared connection —
/// that is the recommended RabbitMQ.Client usage pattern because channels are cheap
/// but connections are expensive (one TCP socket per connection).
/// </para>
///
/// <para>
/// Configuration is read from the <c>RabbitMQ</c> section of appsettings.json:
/// <code>
/// "RabbitMQ": {
///   "Host":        "localhost",
///   "Port":        "5672",
///   "Username":    "guest",
///   "Password":    "guest",
///   "VirtualHost": "/"
/// }
/// </code>
/// In Docker, override <c>Host</c> with the container name (e.g. "rabbitmq")
/// via environment variables: <c>RabbitMQ__Host=rabbitmq</c>.
/// </para>
/// </summary>
public static class RabbitMqConnectionFactory
{
    /// <summary>
    /// Asynchronously creates a connected <see cref="IConnection"/> using settings
    /// from the <c>RabbitMQ</c> configuration section.
    /// </summary>
    public static async Task<IConnection> CreateAsync(IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName  = section["Host"]        ?? "localhost",
            Port      = int.Parse(section["Port"] ?? "5672"),
            UserName  = section["Username"]    ?? "guest",
            Password  = section["Password"]    ?? "guest",
            VirtualHost = section["VirtualHost"] ?? "/",

            // Automatically reconnect after network blips.
            AutomaticRecoveryEnabled = true,

            // Re-declare exchanges/queues and re-register consumers after reconnect.
            TopologyRecoveryEnabled  = true,
        };

        return await factory.CreateConnectionAsync();
    }
}
