using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Orchestrator.Repositories
{
    public class AgentTeamRepository : IAgentTeamRepository
    {
        private readonly AgentForgeDbContext _context;

        public AgentTeamRepository(AgentForgeDbContext context)
        {
            _context = context;
        }

        public async Task<AgentTeam?> RetrieveAsync(Guid id)
        {
            return await _context.AgentTeams
                .Include(t => t.Agents)
                .FirstOrDefaultAsync(t => t.AgentTeamId == id);
        }

        public async Task<IEnumerable<AgentTeam>> RetrieveAsync()
        {
            return await _context.AgentTeams
                .Include(t => t.Agents)
                .ToListAsync();
        }

        public async Task CreateAsync(AgentTeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            await _context.AgentTeams.AddAsync(team);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AgentTeam team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            _context.AgentTeams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var team = await _context.AgentTeams.FindAsync(id);
            if (team != null)
            {
                _context.AgentTeams.Remove(team);
                await _context.SaveChangesAsync();
            }
        }
    }
}
