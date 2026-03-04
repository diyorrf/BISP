using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.Data.Entities
{
    public class EmailConfirmationToken
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }

        public User User { get; set; } = null!;
    }
}