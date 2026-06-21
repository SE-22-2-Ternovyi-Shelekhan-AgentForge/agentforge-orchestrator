using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Repositories;

public interface IAgentSessionTraceRepository
{
    Task AddAsync(AgentSessionTrace trace);
    Task<AgentSessionTrace?> RetrieveBySessionAsync(Guid sessionId);
}
