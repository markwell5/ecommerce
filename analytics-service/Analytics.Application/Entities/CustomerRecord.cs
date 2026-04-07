using System;

namespace Analytics.Application.Entities
{
    public class CustomerRecord
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}
