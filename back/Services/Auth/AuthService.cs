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

        public async Task<(string? Token, string? Error)> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                return (null, "No account found with this email address");

            if (!user.EmailConfirmed)
                return (null, "Please verify your email before logging in. Check your inbox for the verification code.");

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (result != PasswordVerificationResult.Success)
                return (null, "Incorrect password. Please try again.");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return (GenerateJwtToken(user), null);
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

            await GenerateAndSendCode(user);

            return true;
        }

        public async Task<string?> VerifyCodeAsync(string email, string code)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.EmailConfirmed)
                return null;

            var confirmation = await _emailRepo.GetByUserIdAsync(user.Id);
            if (confirmation == null || confirmation.Token != code || confirmation.ExpiresAt < DateTime.UtcNow)
                return null;

            user.EmailConfirmed = true;
            await _emailRepo.RemoveAsync(confirmation);
            await _emailRepo.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();

            return GenerateJwtToken(user);
        }

        public async Task<bool> ResendCodeAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.EmailConfirmed)
                return false;

            await _emailRepo.RemoveByUserIdAsync(user.Id);
            await _emailRepo.SaveChangesAsync();

            await GenerateAndSendCode(user);

            return true;
        }

        private async Task GenerateAndSendCode(User user)
        {
            var code = Random.Shared.Next(100000, 999999).ToString();

            var token = new EmailConfirmationToken
            {
                UserId = user.Id,
                Token = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _emailRepo.AddAsync(token);
            await _emailRepo.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Your LegalGuard verification code",
                $@"
                <div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 32px;'>
                    <h2 style='color: #1e293b; margin-bottom: 8px;'>Welcome to LegalGuard</h2>
                    <p style='color: #64748b; font-size: 15px;'>Use the code below to verify your email address:</p>
                    <div style='margin: 24px 0; padding: 20px; background: #f1f5f9; border-radius: 12px; text-align: center;'>
                        <span style='font-size: 36px; font-weight: 700; letter-spacing: 8px; color: #1e40af;'>{code}</span>
                    </div>
                    <p style='color: #94a3b8; font-size: 13px;'>This code expires in 10 minutes. If you didn't create an account, you can ignore this email.</p>
                </div>
                ");
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