using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Products.Infrastructure.Messaging;

/// <summary>
/// Creates and returns a singleton <see cref="IConnection"/> to RabbitMQ.
/// The connection is shared across <see cref="RabbitMqEventBus"/> (publisher)
/// and all consumer background services. Each consumer/publisher opens its own
/// <see cref="IChannel"/> from the shared connection — this is the recommended
/// RabbitMQ.Client usage pattern.
/// </summary>
public static class RabbitMqConnectionFactory
{
    /// <summary>
    /// Builds a <see cref="IConnection"/> from the <c>RabbitMQ</c> configuration section.
    /// Expected keys: <c>Host</c>, <c>Port</c>, <c>Username</c>, <c>Password</c>, <c>VirtualHost</c>.
    /// </summary>
    public static async Task<IConnection> CreateAsync(IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = section["Host"] ?? "localhost",
            Port = int.Parse(section["Port"] ?? "5672"),
            UserName = section["Username"] ?? "guest",
            Password = section["Password"] ?? "guest",
            VirtualHost = section["VirtualHost"] ?? "/",

            // Automatic connection recovery reconnects after network blips.
            AutomaticRecoveryEnabled = true,

            // Recover consumer subscriptions after reconnect.
            TopologyRecoveryEnabled = true,
        };

        return await factory.CreateConnectionAsync();
    }
}