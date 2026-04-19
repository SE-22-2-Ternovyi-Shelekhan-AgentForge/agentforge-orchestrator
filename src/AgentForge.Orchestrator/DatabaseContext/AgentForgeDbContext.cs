using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Orchestrator.DatabaseContext
{
    public class AgentForgeDbContext : DbContext
    {
        public AgentForgeDbContext(DbContextOptions<AgentForgeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<AgentTeam> AgentTeams { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Conversation) 
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Team)
                .WithMany(t => t.Conversations)
                .HasForeignKey(c => c.AgentTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Team)
                .WithMany(t => t.Agents)
                .HasForeignKey(a => a.AgentTeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}