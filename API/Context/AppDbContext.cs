using API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using System.Text;

namespace API.Context
{
    public class AppDbContext : IdentityDbContext<User>
    {

        // Constructor
        private readonly IHttpContextAccessor _contextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options,IHttpContextAccessor contextAccessor) : base(options) 
        { 
            _contextAccessor = contextAccessor;
        }

        // Methods

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var modifiedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var item in modifiedEntities)
            {
                // Use the null-conditional operator (?) and null-coalescing (??)
                var userEmail = _contextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
                                ?? "System";

                var auditLog = new AuditLog()
                {
                    EntityName = item.Entity.GetType().Name,
                    UserEmail = userEmail,
                    Action = item.State.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Changes = GetChanges(item)
                };

                // Ensure the AuditLogs DbSet/List is initialized
                AuditLogs.Add(auditLog);
            }

            return base.SaveChangesAsync(cancellationToken);
        }
        private string GetChanges(EntityEntry item)
        {
            var changes = new StringBuilder();

            foreach (var item1 in item.OriginalValues.Properties)
            {
                var originalValue = item.OriginalValues[item1];
                var currentValue = item.CurrentValues[item1];
                if(!Equals(originalValue,currentValue)) 
                { 
                    changes.AppendLine($"{item1.Name}: From {originalValue} to {currentValue}"); 
                }  
            }
            return changes.ToString();
        }

        // Tables

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }

    }
}
