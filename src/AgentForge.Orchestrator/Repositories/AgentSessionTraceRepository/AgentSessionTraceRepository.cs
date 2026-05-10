using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Repositories;

public class AgentSessionTraceRepository : IAgentSessionTraceRepository
{
    private readonly AgentForgeDbContext _context;

    public AgentSessionTraceRepository(AgentForgeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AgentSessionTrace trace)
    {
        await _context.AgentSessionTraces.AddAsync(trace);
        await _context.SaveChangesAsync();
    }
}
