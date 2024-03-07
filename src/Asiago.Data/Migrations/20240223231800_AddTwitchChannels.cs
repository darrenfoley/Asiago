using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asiago.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTwitchChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TwitchChannels",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchChannels", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigurationTwitchChannels",
                columns: table => new
                {
                    SubscribedGuildsGuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TwitchChannelsUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigurationTwitchChannels", x => new { x.SubscribedGuildsGuildId, x.TwitchChannelsUserId });
                    table.ForeignKey(
                        name: "FK_GuildConfigurationTwitchChannels_GuildConfigurations_Subscr~",
                        column: x => x.SubscribedGuildsGuildId,
                        principalTable: "GuildConfigurations",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildConfigurationTwitchChannels_TwitchChannels_TwitchChann~",
                        column: x => x.TwitchChannelsUserId,
                        principalTable: "TwitchChannels",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildConfigurationTwitchChannels_TwitchChannelsUserId",
                table: "GuildConfigurationTwitchChannels",
                column: "TwitchChannelsUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildConfigurationTwitchChannels");

            migrationBuilder.DropTable(
                name: "TwitchChannels");
        }
    }
}
