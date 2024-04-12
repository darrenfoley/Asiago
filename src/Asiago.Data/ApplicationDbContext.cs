using Asiago.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Asiago.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<GuildConfiguration> GuildConfigurations { get; set; }
        public DbSet<TwitchChannel> TwitchChannels { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("asiago");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
