using Apex.Api.Controllers;
using Apex.Application.DTOs;
using Apex.Application.Interfaces;
using Apex.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Apex.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authMock = new Mock<IAuthService>();
        _controller = new AuthController(_authMock.Object);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest("jho", "1318_Http!");
        var expectedResponse = new AuthResponse("fake-jwt-token", "jho", "Admin");

        _authMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualResponse = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("jho", actualResponse.Username);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest("hacker", "wrong_password");
        _authMock.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                 .ReturnsAsync((AuthResponse)null!);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

        var actualError = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);

        Assert.Equal(401, actualError.StatusCode);
        Assert.Equal("Invalid username or password.", actualError.Message);
    }
}