using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Services
{
    public interface IAgentService
    {
        Task<AgentTeamDto> CreateTeamAsync(string name);
        Task<AgentDto> CreateAgentAsync(AgentDto agentDto);
        Task<IEnumerable<AgentTeamDto>> GetAllTeamsAsync();
        Task UpdateAgentAsync(AgentDto agentDto);
        Task UpdateTeamAsync(AgentTeamDto agentTeamDto);
        Task DeleteTeamAsync(Guid teamId);
        Task DeleteAgentAsync(Guid agentId);
    }
}