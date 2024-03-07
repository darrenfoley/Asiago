using Asiago.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asiago.Data.Configurations
{
    public class GuildConfigurationEntityConfiguration : IEntityTypeConfiguration<GuildConfiguration>
    {
        public void Configure(EntityTypeBuilder<GuildConfiguration> builder)
        {
            builder.HasKey(gc => gc.GuildId);
            builder.Property(gc => gc.GuildId).ValueGeneratedNever();
        }
    }
}
