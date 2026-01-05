using Azure.Core;

namespace API.Models
{
    public class RefreshToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Token { get; set; }
        public DateTime? ExpireTime { get; set; }

        public string? UserId { get; set; }

        // Navigation Properties

        public virtual User? User { get; set; }
    }
}
