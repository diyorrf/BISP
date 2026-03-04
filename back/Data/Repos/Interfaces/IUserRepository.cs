using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;

namespace back.Data.Repos.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
    }
}