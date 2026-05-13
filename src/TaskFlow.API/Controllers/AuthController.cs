using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.API.Contracts;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Core.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (existingUser is not null)
        {
            return Conflict(ApiResponse<AuthResponse>.Fail("Email already registered."));
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = CreateJwtToken(user);

        var response = new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Registration successful."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid email or password."));
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid email or password."));
        }

        var token = CreateJwtToken(user);

        var response = new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful."));
    }

    private string CreateJwtToken(User user)
    {
        var key = GetConfigurationValue("Jwt:Key", "JWT_KEY");

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT key is missing.");
        }

        if (key.Length < 32)
        {
            throw new InvalidOperationException("JWT key must be at least 32 characters long.");
        }

        var issuer = GetConfigurationValue("Jwt:Issuer", "JWT_ISSUER");
        var audience = GetConfigurationValue("Jwt:Audience", "JWT_AUDIENCE");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string? GetConfigurationValue(string sectionKey, string environmentKey)
    {
        return _configuration[sectionKey]
            ?? _configuration[environmentKey]
            ?? Environment.GetEnvironmentVariable(environmentKey);
    }
}