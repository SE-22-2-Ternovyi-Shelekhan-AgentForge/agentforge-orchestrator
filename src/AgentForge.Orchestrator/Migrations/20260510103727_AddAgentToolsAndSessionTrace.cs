using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentForge.Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentToolsAndSessionTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgentSessionId",
                table: "ChatMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorPrompt",
                table: "AgentTeams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tools",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AgentSessionTraces",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Trace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokensInTotal = table.Column<int>(type: "int", nullable: true),
                    TokensOutTotal = table.Column<int>(type: "int", nullable: true),
                    Iterations = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSessionTraces", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_AgentSessionTraces_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentSessionTraces_ConversationId",
                table: "AgentSessionTraces",
                column: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentSessionTraces");

            migrationBuilder.DropColumn(
                name: "AgentSessionId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "SupervisorPrompt",
                table: "AgentTeams");

            migrationBuilder.DropColumn(
                name: "Tools",
                table: "Agents");
        }
    }
}
