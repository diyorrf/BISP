using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;

namespace back.Data.Repos.Interfaces
{
    public interface IEmailRepo
    {
        Task<EmailConfirmationToken?> GetByTokenAsync(string token);
        Task<EmailConfirmationToken?> GetByUserIdAsync(long userId);
        Task AddAsync(EmailConfirmationToken token);
        Task RemoveAsync(EmailConfirmationToken token);
        Task RemoveByUserIdAsync(long userId);
        Task SaveChangesAsync();
    }
}