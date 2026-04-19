using Auth.Application;
using Auth.Application.Behaviors;
using FluentValidation;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.Interfaces;
using Auth.Domain.Settings;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Auth.Application.Abstractions;
using Auth.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICES CONFIGURATION
// ============================================================

// ------------------------------------------------------------
// 1. DATABASE CONFIGURATION
// Connection to Auth-specific database (separate from Products
// ------------------------------------------------------------
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AuthConnection"),
        // Retry on transient failures (network issues, etc.
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

// ------------------------------------------------------------
// 2. DEPENDENCY INJECTION
// Register services following Clean Architecture layers
// ------------------------------------------------------------

// Bind JwtSettings so IOptions<JwtSettings> works in JwtTokenService
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Domain layer interfaces → Infrastructure implementations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

// Application layer services
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// ------------------------------------------------------------
// 3. JWT AUTHENTICATION
// Configure JWT bearer token validation
// ------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate the issuer (who created the token)
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        // Validate the audience (who the token is for)
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        // Validate the signing key
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        ),

        // Validate token expiration
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No tolerance for expiration
    };
});

// ------------------------------------------------------------
// 4. API CONFIGURATION
// Controllers, Swagger, CORS
// ------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Auth Microservice API",
        Version = "v1",
        Description = "Authentication and Authorization service"
    });
});

// CORS - Allow requests from API Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5000") // API Gateway
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register all MediatR handlers discovered in Auth.Application.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<AssemblyMarker>();

    // Pipeline order matters: Logging wraps everything, Validation short-circuits
    // bad input before Transaction opens a DB transaction.
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

// Auto-discover FluentValidation validators in the Application assembly.
builder.Services.AddValidatorsFromAssemblyContaining<AssemblyMarker>();

var app = builder.Build();

// ============================================================
// MIDDLEWARE PIPELINE
// ============================================================

app.UseMiddleware<ValidationExceptionMiddleware>();

// Development: Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("GatewayPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ============================================================
// DATABASE MIGRATION (Development only)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.Run();
