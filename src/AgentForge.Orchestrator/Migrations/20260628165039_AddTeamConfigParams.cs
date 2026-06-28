using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentForge.Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamConfigParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxIterations",
                table: "AgentTeams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRounds",
                table: "AgentTeams",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxIterations",
                table: "AgentTeams");

            migrationBuilder.DropColumn(
                name: "MaxRounds",
                table: "AgentTeams");
        }
    }
}
