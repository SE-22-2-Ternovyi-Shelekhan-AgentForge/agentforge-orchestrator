using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Repositories
{
    public interface IAgentRepository
    {
        Task<Agent?> RetrieveByIdAsync(Guid id);
        Task<IEnumerable<Agent>> RetrieveByTeamIdAsync(Guid teamId);
        Task CreateAsync(Agent agent);
        Task UpdateAsync(Agent agent);
        Task DeleteAsync(Guid id);
    }
}
