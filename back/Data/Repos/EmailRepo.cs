using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;
using back.Data.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repos
{
    public class EmailRepo : IEmailRepo
    {
        private readonly AppDbContext _context;
        public EmailRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(EmailConfirmationToken token)
        {
            await _context.EmailConfirmationTokens.AddAsync(token);
        }

        public async Task<EmailConfirmationToken?> GetByTokenAsync(string token)
        {
            return await _context.EmailConfirmationTokens
                .Include(t => t.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<EmailConfirmationToken?> GetByUserIdAsync(long userId)
        {
            return await _context.EmailConfirmationTokens
                .Include(t => t.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task RemoveAsync(EmailConfirmationToken token)
        {
            _context.EmailConfirmationTokens.Remove(token);
            await Task.CompletedTask;
        }

        public async Task RemoveByUserIdAsync(long userId)
        {
            var tokens = await _context.EmailConfirmationTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            _context.EmailConfirmationTokens.RemoveRange(tokens);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}