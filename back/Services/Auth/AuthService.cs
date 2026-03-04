using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using back.Data.Entities;
using back.Data.Repos.Interfaces;
using back.Models.DTOs;
using back.Models.Settings;
using back.Services.Auth.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace back.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailRepo _emailRepo;
        private readonly IEmailService _emailService;
        private readonly JWToken _jwtSettings;

        private readonly PasswordHasher<User> _passwordHasher = new();
        public AuthService(IUserRepository userRepository, IEmailRepo emailRepo, IEmailService emailService, IOptions<JWToken> jwtOptions)
        {
            _userRepository = userRepository;
            _emailRepo = emailRepo;
            _emailService = emailService;
            _jwtSettings = jwtOptions.Value;
        }
        public async Task<string?> ConfirmEmailAsync(string token)
        {
            var confirmation = await _emailRepo.GetByTokenAsync(token);
            if (confirmation == null || confirmation.ExpiresAt < DateTime.UtcNow)
                return null;

            confirmation.User.EmailConfirmed = true;
            await _emailRepo.RemoveAsync(confirmation);
            await _emailRepo.SaveChangesAsync();

            return GenerateJwtToken(confirmation.User);
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.EmailConfirmed)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            return result == PasswordVerificationResult.Success ? GenerateJwtToken(user) : null;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.GetByEmailAsync(request.Email) != null)
                return false;

            var user = new User
            {
                Email = request.Email,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Generate token
            var token = new EmailConfirmationToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            await _emailRepo.AddAsync(token);
            await _emailRepo.SaveChangesAsync();

            var confirmationLink =
                // $"https://pathwayed-chere-soppily.ngrok-free.dev/api/auth/confirm-email?token={token.Token}";
                $"http://localhost:5041/api/auth/confirm-email?token={token.Token}";

            // 5️⃣ Send email
            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your email",
                $@"
                    <h2>Welcome to Legal Guagrds</h2>
                    <p>Please click the link below to confirm your email:</p>
                    <a href='{confirmationLink}'>Confirm Email</a>
                    <p>This link will expire in 24 hours.</p>
                ");

            return true;
        }
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}