namespace Apex.Application.DTOs;

public record AuthResponse(
    string Token,
    string Username,
    string Role
);