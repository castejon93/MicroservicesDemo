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

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICES CONFIGURATION
// ============================================================

// ------------------------------------------------------------
// 1. DATABASE CONFIGURATION
// Connection to Auth-specific database (separate from Products)
// ------------------------------------------------------------
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AuthConnection"),
        // Retry on transient failures (network issues, etc.)
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

// CORS - Allow requests from API Gateway and React dev client.
// In production this should be locked to the gateway origin only;
// the React app should never call microservices directly.
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",  // API Gateway (HTTP)
                "https://localhost:5000", // API Gateway (HTTPS)
                "http://localhost:5173",  // React dev server (Vite)
                "http://localhost:3000"   // React dev server (CRA fallback)
              )
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================================
// MIDDLEWARE PIPELINE
// ============================================================

// Development: Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection is intentionally removed.
// Microservices behind an API Gateway should not redirect to HTTPS;
// TLS termination is the gateway's responsibility. Keeping this
// middleware causes the browser to follow a 301 redirect directly
// to the microservice, bypassing the gateway and breaking CORS.
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
