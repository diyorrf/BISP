using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;
using back.Data.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repos
{
    public class DocumentRepository: IDocumentRepository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Documents.FindAsync(new object[] { id }, ct);
        }

        public async Task<Document?> GetByIdAndUserIdAsync(Guid id, long userId, CancellationToken ct = default)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId, ct);
        }

        public async Task<Document?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Documents
                .Include(d => d.Questions)
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task<IEnumerable<Document>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(ct);
        }

        public async Task<int> GetCountAsync(CancellationToken ct = default)
        {
            return await _context.Documents.CountAsync(ct);
        }

        public async Task<int> GetCountByUserIdAsync(long userId, CancellationToken ct = default)
        {
            return await _context.Documents.CountAsync(d => d.UserId == userId, ct);
        }

        public async Task<IEnumerable<Document>> GetAllByUserIdAsync(long userId, CancellationToken ct = default)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Document>> GetRecentByUserIdAsync(long userId, int count, CancellationToken ct = default)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .Take(count)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Document document, CancellationToken ct = default)
        {
            await _context.Documents.AddAsync(document, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Document document, CancellationToken ct = default)
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var document = await GetByIdAsync(id, ct);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> DeleteByIdAndUserIdAsync(Guid id, long userId, CancellationToken ct = default)
        {
            var document = await GetByIdAndUserIdAsync(id, userId, ct);
            if (document == null) return false;
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}