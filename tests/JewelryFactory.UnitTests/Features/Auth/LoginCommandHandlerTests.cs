using FluentAssertions;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Auth.Commands.Login;
using JewelryFactory.Application.Features.Auth.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using JewelryFactory.UnitTests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace JewelryFactory.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtService = new();

    public LoginCommandHandlerTests()
    {
        _jwtService.Setup(x => x.GenerateTokens(It.IsAny<User>()))
            .Returns(("access", "refresh", "refresh-hash",
                DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public async Task Handle_WithCorrectCredentials_ShouldReturnTokens()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Email = "user@example.com",
            PasswordHash = "stored-hash",
            FullName = "User One",
            Role = UserRole.Manager
        });
        await db.SaveChangesAsync();

        _passwordHasher.Setup(x => x.Verify("correct-pass", "stored-hash")).Returns(true);

        var handler = new LoginCommandHandler(db, _passwordHasher.Object, _jwtService.Object,
            NullLogger<LoginCommandHandler>.Instance);

        var result = await handler.Handle(
            new LoginCommand(new LoginRequestDto("user@example.com", "correct-pass")),
            CancellationToken.None);

        result.AccessToken.Should().Be("access");
        result.Role.Should().Be("Manager");
        db.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldThrowUnauthorized()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Email = "user@example.com",
            PasswordHash = "stored-hash",
            FullName = "User",
            Role = UserRole.Worker
        });
        await db.SaveChangesAsync();

        _passwordHasher.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var handler = new LoginCommandHandler(db, _passwordHasher.Object, _jwtService.Object,
            NullLogger<LoginCommandHandler>.Instance);

        var act = () => handler.Handle(
            new LoginCommand(new LoginRequestDto("user@example.com", "wrong")),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ShouldThrowUnauthorized()
    {
        await using var db = TestDbContextFactory.Create();
        var handler = new LoginCommandHandler(db, _passwordHasher.Object, _jwtService.Object,
            NullLogger<LoginCommandHandler>.Instance);

        var act = () => handler.Handle(
            new LoginCommand(new LoginRequestDto("ghost@example.com", "any")),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
