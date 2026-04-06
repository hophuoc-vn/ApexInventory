using Moq;
using Xunit;
using Apex.Infrastructure.Identity;
using Apex.Domain.Interfaces;
using Apex.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Apex.Domain.Entities;

namespace Apex.UnitTests.Identity;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _configMock = new Mock<IConfiguration>();

        // Setup fake JWT config for the test environment
        _configMock.Setup(c => c["Jwt:Key"]).Returns("Super_Secret_Key_For_Testing_12345");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("ApexApi");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("ApexUsers");

        _authService = new AuthService(_uowMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var password = "correct_password";
        var user = new User
        {
            Username = "jho",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Admin"
        };

        // Tell the mock to return our 'jho' user when the service asks for it
        _uowMock.Setup(u => u.Users.GetByUsernameAsync("jho")).ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(new LoginRequest("jho", password));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jho", result.Username);
        Assert.NotEmpty(result.Token);
    }
}