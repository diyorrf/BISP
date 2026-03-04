using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.Data.Entities
{
    public class Question
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public long? UserId { get; set; }
        public Document Document { get; set; } = null!;
        public string QuestionText { get; set; } = string.Empty;
        public string? Answer { get; set; }
        public DateTime AskedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public int TokensUsed { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }
}