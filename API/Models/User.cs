using Microsoft.AspNetCore.Identity;

namespace API.Models
{
    public class User:IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string>? Roles { get; set; }
        public DateTime LastLoginTime { get; set; } = new();

        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
