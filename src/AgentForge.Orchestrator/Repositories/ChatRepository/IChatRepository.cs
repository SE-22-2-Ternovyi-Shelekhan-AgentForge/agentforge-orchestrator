using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Repositories
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatMessage>> RetrieveHistoryAsync(Guid conversationId);
        Task AddMessageAsync(ChatMessage message);
        Task DeleteAsync(Guid id);
    }
}
