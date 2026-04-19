using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentForge.Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class FixChatMessageIdColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatIMessaged",
                table: "ChatMessages",
                newName: "ChatMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChatMessageId",
                table: "ChatMessages",
                newName: "ChatIMessaged");
        }
    }
}
