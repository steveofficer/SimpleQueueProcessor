using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BackgroundQueue
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) {}

        public DbSet<Message> MessageQueue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            var messageQueue = modelBuilder.Entity<Message>();
            messageQueue.Property(m => m.Data).HasConversion(
                x => x.ToString(),
                x => JObject.Parse(x)
            );
        }
    }
}