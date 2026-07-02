using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomerSupport.API.DTOs;
using CustomerSupport.API.Services;
using CustomerSupport.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CustomerSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly MetricsService _metricsService;

    public AuthController(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        MetricsService metricsService)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _metricsService = metricsService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        _logger.LogInformation("Login attempt for email {Email}.", dto.Email);
        _metricsService.RegisterLoginAttempt();
        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user is null)
        {
            _logger.LogWarning("Login failed. User not found for email {Email}.", dto.Email);
            _metricsService.RegisterLoginFailure();
            return Unauthorized("Invalid email or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed. Invalid password for email {Email}.", dto.Email);
            _metricsService.RegisterLoginFailure();
            return Unauthorized("Invalid email or password.");
        }

        if (user.Role is null)
        {
            _logger.LogError("Login failed. User {Email} has no role assigned.", dto.Email);
            _metricsService.RegisterLoginFailure();
            return StatusCode(500, "User role is not assigned.");
        }
        
        var key = _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            key = Environment.GetEnvironmentVariable("JWT_KEY");
        }

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            _logger.LogError("Login failed. JWT configuration is invalid for email {Email}.", dto.Email);
            _metricsService.RegisterLoginFailure();
            return StatusCode(500, "JWT configuration is invalid.");
        }

        var expiresAt = DateTime.UtcNow.AddHours(2);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.Name
        };

        _logger.LogInformation("Login succeeded for email {Email} with role {Role}.", dto.Email, user.Role.Name);
        _metricsService.RegisterLoginSuccess();
        return Ok(response);
    }
}