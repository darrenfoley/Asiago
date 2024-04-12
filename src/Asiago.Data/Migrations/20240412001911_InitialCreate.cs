using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asiago.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "asiago");

            migrationBuilder.CreateTable(
                name: "GuildConfigurations",
                schema: "asiago",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AdminRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    ModRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    TwitchUpdateChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfigurations", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "TwitchChannels",
                schema: "asiago",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SubscriptionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchChannels", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfigurationTwitchChannels",
                schema: "asiago",
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
                        principalSchema: "asiago",
                        principalTable: "GuildConfigurations",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildConfigurationTwitchChannels_TwitchChannels_TwitchChann~",
                        column: x => x.TwitchChannelsUserId,
                        principalSchema: "asiago",
                        principalTable: "TwitchChannels",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildConfigurationTwitchChannels_TwitchChannelsUserId",
                schema: "asiago",
                table: "GuildConfigurationTwitchChannels",
                column: "TwitchChannelsUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildConfigurationTwitchChannels",
                schema: "asiago");

            migrationBuilder.DropTable(
                name: "GuildConfigurations",
                schema: "asiago");

            migrationBuilder.DropTable(
                name: "TwitchChannels",
                schema: "asiago");
        }
    }
}
