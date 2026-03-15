using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace back.Data.Entities
{
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; } = null!;
        public bool EmailConfirmed { get; set; }

        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(25)]
        public string? FullName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int TokensRemaining { get; set; } = 25_000;
        public DateTime? LastTokenResetAt { get; set; }

        [MaxLength(50)]
        public string Plan { get; set; } = "Free";

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}