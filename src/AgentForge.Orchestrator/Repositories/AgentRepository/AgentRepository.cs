using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Orchestrator.Repositories
{
    public class AgentRepository : IAgentRepository
    {
        private readonly AgentForgeDbContext _context;

        public AgentRepository(AgentForgeDbContext context)
        {
            _context = context;
        }

        public async Task<Agent?> RetrieveByIdAsync(Guid id)
        {
            return await _context.Agents
                .Include(a => a.Team)
                .FirstOrDefaultAsync(a => a.AgentId == id);
        }

        public async Task<IEnumerable<Agent>> RetrieveByTeamIdAsync(Guid teamId)
        {
            return await _context.Agents
                .Where(a => a.AgentTeamId == teamId)
                .ToListAsync();
        }

        public async Task CreateAsync(Agent agent)
        {
            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            await _context.Agents.AddAsync(agent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Agent agent)
        {
            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            _context.Agents.Update(agent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent != null)
            {
                _context.Agents.Remove(agent);
                await _context.SaveChangesAsync();
            }
        }
    }
}
