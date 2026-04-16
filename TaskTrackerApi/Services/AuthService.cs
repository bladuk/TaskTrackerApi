using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskTrackerApi.DTO.Auth;
using TaskTrackerApi.Exceptions;
using TaskTrackerApi.Models;
using TaskTrackerApi.Repositories.Interfaces;
using TaskTrackerApi.Services.Interfaces;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TaskTrackerApi.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    ILogger<AuthService> logger
    ) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto data, CancellationToken ct = default)
    {
        User? user = await unitOfWork.Users.GetByUsernameAsync(data.Username, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(data.Password, user.Password))
            throw new UnauthorizedException("Invalid username or password");
        
        logger.LogInformation("User logged in: {UserId}", user.Id);
        return GenerateToken(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto data, CancellationToken ct = default)
    {
        if (await unitOfWork.Users.AnyAsync(u => u.Username == data.Username, ct))
            throw new ConflictException("Username already exists");
        
        User user = new()
        {
            Id = Guid.NewGuid(),
            Username = data.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(data.Password)
        };
        
        await unitOfWork.Users.AddAsync(user, ct);
        await unitOfWork.CommitAsync(ct);
        
        logger.LogInformation("User registered: {UserId}", user.Id);
        return GenerateToken(user);
    }

    private AuthResponseDto GenerateToken(User user)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.Username)
        ];
        
        var jwtConfiguration = configuration.GetSection("Jwt");

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtConfiguration["Key"]));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        
        DateTime expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtConfiguration["ExpiresInMinutes"]));
        JwtSecurityToken token = new(jwtConfiguration["Issuer"], jwtConfiguration["Audience"], claims, expires: expires,
            signingCredentials: credentials);
        
        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        return new AuthResponseDto(tokenString, expires);
    }
}