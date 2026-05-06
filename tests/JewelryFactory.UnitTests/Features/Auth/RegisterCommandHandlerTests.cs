using FluentAssertions;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Auth.Commands.Register;
using JewelryFactory.Application.Features.Auth.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using JewelryFactory.UnitTests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace JewelryFactory.UnitTests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtService = new();

    public RegisterCommandHandlerTests()
    {
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");
        _jwtService.Setup(x => x.GenerateTokens(It.IsAny<User>()))
            .Returns(("access-token", "refresh-token", "refresh-hash",
                DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public async Task Handle_WithNewEmail_ShouldCreateUserAndReturnTokens()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var handler = new RegisterCommandHandler(db, _passwordHasher.Object, _jwtService.Object,
            NullLogger<RegisterCommandHandler>.Instance);

        var request = new RegisterRequestDto(
            Email: "Test@Example.com",
            Password: "SecurePass123",
            FullName: "John Doe",
            Role: UserRole.Sales);

        // Act
        var result = await handler.Handle(new RegisterCommand(request), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com"); // normalized lowercase
        result.FullName.Should().Be("John Doe");
        result.Role.Should().Be(UserRole.Sales.ToString());
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");

        db.Users.Should().HaveCount(1);
        db.RefreshTokens.Should().HaveCount(1);
        _passwordHasher.Verify(x => x.Hash("SecurePass123"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowBusinessRuleException()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(new User
        {
            Email = "exists@example.com",
            PasswordHash = "x",
            FullName = "Existing",
            Role = UserRole.Worker
        });
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db, _passwordHasher.Object, _jwtService.Object,
            NullLogger<RegisterCommandHandler>.Instance);

        var request = new RegisterRequestDto(
            Email: "EXISTS@example.com", // different case — should still match
            Password: "SecurePass123",
            FullName: "Duplicate",
            Role: UserRole.Sales);

        // Act
        var act = () => handler.Handle(new RegisterCommand(request), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already registered*");
    }
}
