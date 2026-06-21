using System;
using System.Threading.Tasks;
using AgentForge.Orchestrator.Configuration;
using AgentForge.Orchestrator.Exceptions;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Repositories;
using AgentForge.Orchestrator.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AgentForge.Orchestrator.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var jwt = Options.Create(new JwtSettings
            {
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                Key = "test-signing-key-that-is-long-enough-32+chars!!",
                ExpiryMinutes = 60,
            });
            _authService = new AuthService(_userRepoMock.Object, jwt);
        }

        [Fact]
        public async Task RegisterAsync_NewEmail_PersistsUserAndReturnsToken()
        {
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);

            var result = await _authService.RegisterAsync(new RegisterRequest
            {
                Email = "New@Example.com",
                Password = "secret123",
                DisplayName = "New User",
            });

            result.Token.Should().NotBeNullOrWhiteSpace();
            result.Email.Should().Be("new@example.com"); // normalized to lower case

            _userRepoMock.Verify(r => r.CreateAsync(It.Is<User>(u =>
                u.Email == "new@example.com" &&
                !string.IsNullOrEmpty(u.PasswordHash) &&
                u.PasswordHash != "secret123")), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsAndDoesNotPersist()
        {
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(true);

            Func<Task> act = () => _authService.RegisterAsync(new RegisterRequest
            {
                Email = "dup@example.com",
                Password = "secret123",
                DisplayName = "Dup",
            });

            await act.Should().ThrowAsync<EmailAlreadyExistsException>();
            _userRepoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            var user = new User { UserId = Guid.NewGuid(), Email = "user@example.com", DisplayName = "User" };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "secret123");
            _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

            var result = await _authService.LoginAsync(new LoginRequest
            {
                Email = "user@example.com",
                Password = "secret123",
            });

            result.Token.Should().NotBeNullOrWhiteSpace();
            result.UserId.Should().Be(user.UserId);
        }

        [Fact]
        public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentials()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            Func<Task> act = () => _authService.LoginAsync(new LoginRequest { Email = "x@y.z", Password = "p" });

            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
        {
            var user = new User { UserId = Guid.NewGuid(), Email = "user@example.com", DisplayName = "User" };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "correct-password");
            _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);

            Func<Task> act = () => _authService.LoginAsync(new LoginRequest
            {
                Email = "user@example.com",
                Password = "wrong-password",
            });

            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }
    }
}
