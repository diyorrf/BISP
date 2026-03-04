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

        /// <summary>Remaining tokens/credits for AI usage. Refreshed per plan.</summary>
        public int TokensRemaining { get; set; } = 10_000;

        /// <summary>Plan name: Free, Pro, Enterprise.</summary>
        [MaxLength(50)]
        public string Plan { get; set; } = "Free";

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}