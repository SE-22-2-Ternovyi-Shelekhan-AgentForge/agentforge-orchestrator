using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation?> RetrieveAsync(Guid id);
        Task<IEnumerable<Conversation>> RetrieveByUserIdAsync(Guid userId);
        Task CreateAsync(Conversation conversation);
        Task UpdateAsync(Conversation conversation);
        Task DeleteAsync(Guid id);
    }
}
