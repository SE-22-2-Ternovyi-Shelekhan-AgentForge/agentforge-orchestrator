using System.Security.Claims;
using System.Text;
using AgentForge.Orchestrator.Configuration;
using AgentForge.Orchestrator.Exceptions;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgentForge.Orchestrator.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly JwtSettings _jwt;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthService(IUserRepository users, IOptions<JwtSettings> jwt)
        {
            _users = users;
            _jwt = jwt.Value;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _users.ExistsByEmailAsync(email))
                throw new EmailAlreadyExistsException($"A user with email '{email}' already exists.");

            var user = new User
            {
                Email = email,
                DisplayName = request.DisplayName,
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _users.CreateAsync(user);

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _users.GetByEmailAsync(request.Email)
                ?? throw new InvalidCredentialsException("Invalid email or password.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new InvalidCredentialsException("Invalid email or password.");

            return BuildAuthResponse(user);
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

            var subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.DisplayName ?? user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            });

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience,
                Subject = subject,
                Expires = expires,
                SigningCredentials = creds,
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);

            return new AuthResponse
            {
                Token = token,
                ExpiresAtUtc = expires,
                UserId = user.UserId,
                Email = user.Email,
                DisplayName = user.DisplayName,
            };
        }
    }
}
