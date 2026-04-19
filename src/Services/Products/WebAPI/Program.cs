using Products.Application.Interfaces;
using Products.Application.Services;
using Products.Domain.Interfaces;
using Products.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Products.Infrastructure.Data;
using System.Text;

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
builder.Services.AddScoped<IProductService, ProductService>();

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
});

// CORS - Allow API Gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================================
// MIDDLEWARE PIPELINE
// ============================================================

if (app.Environment.IsDevelopment())
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

