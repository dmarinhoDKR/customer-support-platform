using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CustomerSupport.Domain.Entities;
using CustomerSupport.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CustomerSupport.API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
        ?? Environment.GetEnvironmentVariable("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DefaultConnection was not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            key = Environment.GetEnvironmentVariable("JWT_KEY");
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT_KEY was not configured.");
        }
        
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
var logger = app.Logger;

logger.LogInformation("CustomerSupport API starting in {Environment} environment.", app.Environment.EnvironmentName);
logger.LogInformation("Preparing database and seed data.");


// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (dbContext.Database.IsRelational())
    {
        logger.LogInformation("Relational database detected. Applying migrations if needed.");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations checked successfully.");
    }
    else
    {
        logger.LogInformation("Non-relational test database detected. Ensuring database is created.");
        dbContext.Database.EnsureCreated();
        logger.LogInformation("Test database ensured successfully.");
    }

    if (!dbContext.Roles.Any())
    { 
        logger.LogInformation("Seeding default roles.");
        dbContext.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "Agent" },
            new Role { Name = "Customer" }
        
        );
    
        dbContext.SaveChanges();
        logger.LogInformation("Default roles seeded successfully.");
    }

    if (!dbContext.Categories.Any())
    {
        logger.LogInformation("Seeding default categories.");
        dbContext.Categories.AddRange(
            new Category { Name = "Authentication", Description = "Login, access, and identity issues." },
            new Category { Name = "Billing", Description = "Payments, invoices, and subscription issues." },
            new Category { Name = "Technical Support", Description = "Application errors and technical incidents." },
            new Category { Name = "General Support", Description = "General customer service requests." }
        );

        dbContext.SaveChanges();
        logger.LogInformation("Default categories seeded successfully.");
    }

    if (!dbContext.Users.Any())
    {
        logger.LogInformation("Seeding default users.");
        var adminRole = dbContext.Roles.First(x => x.Name == "Admin");
        var agentRole = dbContext.Roles.First(x => x.Name == "Agent");
        var customerRole = dbContext.Roles.First(x => x.Name == "Customer");

        dbContext.Users.AddRange(
            new User
            {
                FullName = "Admin User",
                Email = "admin@customersupport.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                RoleId = adminRole.Id 
            },
            new User
            {
                FullName = "Agent User",
                Email = "agent@customersupport.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                RoleId = agentRole.Id
            },
            new User
            {
                FullName = "Customer User",
                Email = "customer@customersupport.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                RoleId = customerRole.Id
            }
        );

        dbContext.SaveChanges();
        logger.LogInformation("Default users seeded successfully.");
    }

}

logger.LogInformation("Startup initialization completed successfully.");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
