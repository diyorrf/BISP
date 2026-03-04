using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using back.Models.DTOs;
using back.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.RegisterAsync(request);

            if (!success)
                return BadRequest(new
                {
                    Success = false,
                    Message = "User with this email already exists"
                });

            return Ok(new
            {
                Success = true,
                Message = "Registration successful. Please check your email to confirm your account."
            });
        }
        [HttpPost("log-in")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.LoginAsync(request);

            if (token == null)
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Invalid credentials or email not confirmed"
                });

            return Ok(new
            {
                Success = true,
                Token = token
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var jwt = await _authService.ConfirmEmailAsync(token);

            if (jwt == null)
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid or expired token"
                });

            return Ok(new
            {
                Success = true,
                Token = jwt,
                Message = "Email confirmed successfully"
            });
        }
    }
}