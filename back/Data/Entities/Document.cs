using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.Data.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public long? UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}