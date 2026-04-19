// ============================================================
// FILE: src/Gateway/ApiGateway/Program.cs
// PURPOSE: API Gateway - Single entry point for all microservices
// LAYER: Infrastructure (Cross-cutting concern)
// PORT: 5000
// ============================================================

using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// OCELOT CONFIGURATION
// Ocelot is a .NET API Gateway that handles:
// - Request routing to microservices
// - Load balancing
// - Authentication forwarding
// - Rate limiting
// ============================================================

// Load Ocelot configuration from ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ============================================================
// JWT AUTHENTICATION
// Scheme name MUST match AuthenticationProviderKey in ocelot.json ("Bearer").
// Values MUST match what Auth.WebAPI uses to sign tokens.
// ============================================================
const string AuthScheme = "Bearer";

var jwt = builder.Configuration.GetSection("JwtSettings");
var secret = jwt["SecretKey"] ?? throw new InvalidOperationException(
    "Missing 'JwtSettings:SecretKey' in ApiGateway configuration.");
var issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Missing 'JwtSettings:Issuer'.");
var audience = jwt["Audience"] ?? throw new InvalidOperationException("Missing 'JwtSettings:Audience'.");

builder.Services
    .AddAuthentication(AuthScheme)
    .AddJwtBearer(AuthScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

// Add Ocelot services
builder.Services.AddOcelot();

// CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================================
// MIDDLEWARE PIPELINE
// ============================================================

app.UseCors("AllowAll");

// Ocelot must be last in the pipeline
// It handles all routing to downstream services
app.UseAuthentication();
await app.UseOcelot();

app.Run();