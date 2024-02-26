using Asiago.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asiago.Data.Configurations
{
    internal class TwitchChannelEntityConfiguration : IEntityTypeConfiguration<TwitchChannel>
    {
        public void Configure(EntityTypeBuilder<TwitchChannel> builder)
        {
            builder.HasKey(tc => tc.UserId);
            builder.Property(tc => tc.UserId).ValueGeneratedNever();

            builder.HasMany(tc => tc.SubscribedGuilds)
                .WithMany(gc => gc.TwitchChannels)
                .UsingEntity("GuildConfigurationTwitchChannels");
        }
    }
}
