using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentForge.Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Conversations",
                newName: "ConversationId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ChatMessages",
                newName: "ChatIMessaged");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AgentTeams",
                newName: "AgentTeamId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Agents",
                newName: "AgentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConversationId",
                table: "Conversations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ChatIMessaged",
                table: "ChatMessages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AgentTeamId",
                table: "AgentTeams",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "Agents",
                newName: "Id");
        }
    }
}
