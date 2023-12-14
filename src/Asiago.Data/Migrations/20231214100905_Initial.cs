using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asiago.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildConfigurations",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildConfigurations");
        }
    }
}
