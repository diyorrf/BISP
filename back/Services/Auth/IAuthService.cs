using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models.DTOs;

namespace back.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<string?> LoginAsync(LoginRequest request);
        Task<string?> ConfirmEmailAsync(string token);
    }
}