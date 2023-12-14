using Asiago.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Asiago.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<GuildConfiguration> GuildConfigurations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
