using System.Text.Json;
using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        public DbSet<AgentSessionTrace> AgentSessionTraces { get; set; }

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

            modelBuilder.Entity<AgentSessionTrace>()
                .HasOne(t => t.Conversation)
                .WithMany()
                .HasForeignKey(t => t.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            var stringListComparer = new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                v => v.ToList());

            modelBuilder.Entity<Agent>()
                .Property(a => a.Tools)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new())
                .Metadata.SetValueComparer(stringListComparer);

            modelBuilder.Entity<AgentSessionTrace>()
                .Property(t => t.Trace)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<TraceEntryRecord>>(v, (JsonSerializerOptions?)null) ?? new())
                .Metadata.SetValueComparer(new ValueComparer<List<TraceEntryRecord>>(
                    (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) ==
                              JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
                    v => v.GetHashCode(),
                    v => JsonSerializer.Deserialize<List<TraceEntryRecord>>(
                        JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        (JsonSerializerOptions?)null) ?? new()));
        }
    }
}