using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Orchestrator.DatabaseContext
{
    public class AgentForgeDbContext : DbContext
    {
        public AgentForgeDbContext(DbContextOptions<AgentForgeDbContext> options) : base(options) { }

        public DbSet<AgentTeam> Teams { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AgentTeam>()
                .HasMany(t => t.Agents)
                .WithOne()
                .HasForeignKey(a => a.TeamId);
        }
    }
}