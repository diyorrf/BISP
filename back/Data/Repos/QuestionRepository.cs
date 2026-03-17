using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;
using back.Data.Repos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repos
{
    public class QuestionRepository: IQuestionRepository
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Questions.FindAsync(new object[] { id }, ct);
        }

        public async Task<IEnumerable<Question>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
        {
            return await _context.Questions
                .Where(q => q.DocumentId == documentId)
                .OrderByDescending(q => q.AskedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Question>> GetByDocumentIdAndUserIdAsync(Guid documentId, long userId, CancellationToken ct = default)
        {
            return await _context.Questions
                .Where(q => q.DocumentId == documentId && q.UserId == userId)
                .OrderBy(q => q.AskedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Question>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Questions
                .OrderByDescending(q => q.AskedAt)
                .ToListAsync(ct);
        }

        public async Task<int> GetCountAsync(CancellationToken ct = default)
        {
            return await _context.Questions.CountAsync(ct);
        }

        public async Task<int> GetCountByUserIdAsync(long userId, CancellationToken ct = default)
        {
            return await _context.Questions.CountAsync(q => q.UserId == userId, ct);
        }

        public async Task<IEnumerable<Question>> GetRecentByUserIdAsync(long userId, int count, CancellationToken ct = default)
        {
            return await _context.Questions
                .Where(q => q.UserId == userId)
                .Include(q => q.Document)
                .OrderByDescending(q => q.AskedAt)
                .Take(count)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Question question, CancellationToken ct = default)
        {
            await _context.Questions.AddAsync(question, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Question question, CancellationToken ct = default)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var question = await GetByIdAsync(id, ct);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}