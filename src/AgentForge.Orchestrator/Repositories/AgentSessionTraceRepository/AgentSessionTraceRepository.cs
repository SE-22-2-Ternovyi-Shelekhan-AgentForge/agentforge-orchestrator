using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<AgentSessionTrace?> RetrieveBySessionAsync(Guid sessionId)
    {
        return await _context.AgentSessionTraces
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.SessionId == sessionId);
    }
}
