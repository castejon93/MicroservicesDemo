using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Products.Application;
using Products.Application.Abstractions;
using Products.Application.Behaviors;
using Products.Domain.Interfaces;
using Products.Infrastructure.Data;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;
using System.Text;
using Products.Infrastructure.Consumers;
using Products.Infrastructure.Messaging;
using Microsoft.OpenApi;
using Products.Infrastructure.Outbox;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICES CONFIGURATION
// ============================================================

// ------------------------------------------------------------
// 1. DATABASE CONFIGURATION
// Separate ProductsDb database
// ------------------------------------------------------------
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ProductsConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)
    )
);

// ------------------------------------------------------------
// 2. DEPENDENCY INJECTION
// ------------------------------------------------------------
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IDomainEventCollector, EfDomainEventCollector>();
builder.Services.AddScoped<IOutboxRepository, EfOutboxRepository>();

// ------------------------------------------------------------
// RABBITMQ — Singleton connection shared by publisher + consumers
// ------------------------------------------------------------
builder.Services.AddSingleton<IConnection>(_ =>
    RabbitMqConnectionFactory.CreateAsync(builder.Configuration).GetAwaiter().GetResult());

// IEventBus — used by OutboxProcessor to publish integration events.
// Singleton because RabbitMqEventBus holds a single channel internally.
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();

// ------------------------------------------------------------
// OUTBOX PROCESSOR — background worker that reads Outbox and publishes to RabbitMQ
// ------------------------------------------------------------
builder.Services.AddHostedService<OutboxProcessor>();

// ------------------------------------------------------------
// CONSUMERS — background workers that receive integration events
// ------------------------------------------------------------
builder.Services.AddHostedService<UserRegisteredConsumer>();

// ------------------------------------------------------------
// 3. JWT AUTHENTICATION
// IMPORTANT: Must use SAME settings as Auth microservice!
// This allows tokens issued by Auth to be validated here.
// ------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        ),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ------------------------------------------------------------
// 4. API CONFIGURATION
// ------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Products Microservice API",
        Version = "v1",
        Description = "Product catalog management service"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without 'Bearer ' prefix)"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});

// CORS - Allow API Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5001", "http://localhost:5002") // Add other origins as needed
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ------------------------------------------------------------
// MEDIATR — add DomainEventBehavior INSIDE TransactionBehavior
// Pipeline order: Logging → Validation → Transaction → DomainEvent → Handler
// ------------------------------------------------------------
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<AssemblyMarker>();

    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
    cfg.AddOpenBehavior(typeof(DomainEventBehavior<,>)); // ← NEW: inside the transaction
});

builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();

var app = builder.Build();

app.UseMiddleware<ValidationExceptionMiddleware>();

// ============================================================
// MIDDLEWARE PIPELINE
// ============================================================

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("GatewayPolicy");
app.UseAuthentication();  // Validate JWT tokens
app.UseAuthorization();   // Check [Authorize] attributes
app.MapControllers();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    db.Database.Migrate();
}

app.Run();

