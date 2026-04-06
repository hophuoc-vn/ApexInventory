using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

using Apex.Application.DTOs;
using Apex.Application.Interfaces;
using Apex.Domain.Interfaces;

namespace Apex.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public string HashPassword(string password) => BC.HashPassword(password);

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Ensure your IUnitOfWork has the 'Users' property defined!
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

        if (user == null || !BC.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);
        return new AuthResponse(token, user.Username, user.Role);
    }

    private string GenerateJwtToken(Apex.Domain.Entities.User user)
    {
        // Safety check for the Secret Key
        var keyStr = _config["Jwt:Key"] ?? "Default_Secret_Key_At_Least_32_Chars_Long";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3), // Use UtcNow for consistency
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}