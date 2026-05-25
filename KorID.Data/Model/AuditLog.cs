using System;

namespace KorID.Data.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int StatusCode { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}