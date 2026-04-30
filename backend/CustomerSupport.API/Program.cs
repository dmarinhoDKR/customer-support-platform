using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CustomerSupport.Domain.Entities;
using CustomerSupport.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? string.Empty;
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
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();


// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!dbContext.Roles.Any())
    { 
        dbContext.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "Agent" },
            new Role { Name = "Customer" }
        
        );
    
        dbContext.SaveChanges();
    }

    if (!dbContext.Users.Any())
    {
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
    }

}

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

app.Run();
