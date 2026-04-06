using Apex.Application.DTOs;

namespace Apex.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    string HashPassword(string password); // Useful for seeding users
}