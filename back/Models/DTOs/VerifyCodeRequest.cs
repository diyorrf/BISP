using System.ComponentModel.DataAnnotations;

namespace back.Models.DTOs;

public class VerifyCodeRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = null!;
}

public class ResendCodeRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}
