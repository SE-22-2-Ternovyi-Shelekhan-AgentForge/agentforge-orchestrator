using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Repositories
{
    public interface IAgentTeamRepository
    {
        Task<AgentTeam?> RetrieveAsync(Guid id);
        Task<IEnumerable<AgentTeam>> RetrieveAsync();
        Task CreateAsync(AgentTeam team);
        Task UpdateAsync(AgentTeam team);
        Task DeleteAsync(Guid id);
    }
}
