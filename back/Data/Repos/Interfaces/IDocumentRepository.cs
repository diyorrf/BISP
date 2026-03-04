using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;

namespace back.Data.Repos.Interfaces
{
    public interface IDocumentRepository
    {
        Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Document?> GetByIdAndUserIdAsync(Guid id, long userId, CancellationToken ct = default);
        Task<Document?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Document>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<Document>> GetAllByUserIdAsync(long userId, CancellationToken ct = default);
        Task<int> GetCountAsync(CancellationToken ct = default);
        Task<int> GetCountByUserIdAsync(long userId, CancellationToken ct = default);
        Task<IEnumerable<Document>> GetRecentByUserIdAsync(long userId, int count, CancellationToken ct = default);
        Task AddAsync(Document document, CancellationToken ct = default);
        Task UpdateAsync(Document document, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<bool> DeleteByIdAndUserIdAsync(Guid id, long userId, CancellationToken ct = default);
    }
}