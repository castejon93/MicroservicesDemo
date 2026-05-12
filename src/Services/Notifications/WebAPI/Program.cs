using Notifications.Infrastructure.Consumers;
using Notifications.Infrastructure.Messaging;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICES CONFIGURATION
// ============================================================

// ------------------------------------------------------------
// 1. RABBITMQ — Singleton connection shared by all consumers
//
// IConnection is expensive (one TCP socket). Create it once and
// share it. Each consumer opens its own IChannel from it.
//
// RabbitMqConnectionFactory reads from the "RabbitMQ" config section:
//   Host, Port, Username, Password, VirtualHost
// In Docker, these are overridden via environment variables:
//   RabbitMQ__Host=rabbitmq  (container name, not localhost)
// ------------------------------------------------------------
builder.Services.AddSingleton<IConnection>(_ =>
    RabbitMqConnectionFactory
        .CreateAsync(builder.Configuration)
        .GetAwaiter()
        .GetResult());

// ------------------------------------------------------------
// 2. CONSUMERS — background services that receive RabbitMQ messages
//
// AddHostedService registers a BackgroundService that starts when
// the host starts and stops gracefully when the host shuts down.
//
// Add one line per integration event you want to consume.
// Each consumer is independent: its own channel, its own queue,
// its own error handling.
// ------------------------------------------------------------
builder.Services.AddHostedService<ProductCreatedConsumer>();

// ------------------------------------------------------------
// 3. HEALTH CHECKS
//
// Exposes GET /health → 200 OK when the service is running.
// Used by docker-compose depends_on health checks and load balancers.
// Extend with .AddRabbitMQ() from AspNetCore.HealthChecks.RabbitMQ
// to also probe the broker connection.
// ------------------------------------------------------------
builder.Services.AddHealthChecks();

// ============================================================
// MIDDLEWARE PIPELINE
// ============================================================
var app = builder.Build();

// Map the health check endpoint.
// In Docker Compose the healthcheck calls: curl http://localhost:8080/health
app.MapHealthChecks("/health");

app.Run();
