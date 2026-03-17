using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;

namespace back.Data.Repos.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Question>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
        Task<IEnumerable<Question>> GetByDocumentIdAndUserIdAsync(Guid documentId, long userId, CancellationToken ct = default);
        Task<IEnumerable<Question>> GetAllAsync(CancellationToken ct = default);
        Task<int> GetCountAsync(CancellationToken ct = default);
        Task<int> GetCountByUserIdAsync(long userId, CancellationToken ct = default);
        Task<IEnumerable<Question>> GetRecentByUserIdAsync(long userId, int count, CancellationToken ct = default);
        Task AddAsync(Question question, CancellationToken ct = default);
        Task UpdateAsync(Question question, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}