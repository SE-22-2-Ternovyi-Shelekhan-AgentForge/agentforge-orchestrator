using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentForge.Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class FixConversationIdShadowState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agents_AgentTeams_TeamId",
                table: "Agents");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AgentTeams_TeamId",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "Conversations",
                newName: "AgentTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Conversations_TeamId",
                table: "Conversations",
                newName: "IX_Conversations_AgentTeamId");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "Agents",
                newName: "AgentTeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Agents_TeamId",
                table: "Agents",
                newName: "IX_Agents_AgentTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_AgentTeams_AgentTeamId",
                table: "Agents",
                column: "AgentTeamId",
                principalTable: "AgentTeams",
                principalColumn: "AgentTeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AgentTeams_AgentTeamId",
                table: "Conversations",
                column: "AgentTeamId",
                principalTable: "AgentTeams",
                principalColumn: "AgentTeamId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agents_AgentTeams_AgentTeamId",
                table: "Agents");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AgentTeams_AgentTeamId",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "AgentTeamId",
                table: "Conversations",
                newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Conversations_AgentTeamId",
                table: "Conversations",
                newName: "IX_Conversations_TeamId");

            migrationBuilder.RenameColumn(
                name: "AgentTeamId",
                table: "Agents",
                newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Agents_AgentTeamId",
                table: "Agents",
                newName: "IX_Agents_TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agents_AgentTeams_TeamId",
                table: "Agents",
                column: "TeamId",
                principalTable: "AgentTeams",
                principalColumn: "AgentTeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AgentTeams_TeamId",
                table: "Conversations",
                column: "TeamId",
                principalTable: "AgentTeams",
                principalColumn: "AgentTeamId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
