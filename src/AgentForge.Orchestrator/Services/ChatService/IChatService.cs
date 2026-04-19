using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Models.Broker;

namespace AgentForge.Orchestrator.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ConversationDto>> GetUserChatsAsync(Guid userId);
        Task<Guid> CreateEmptyChatAsync(Guid userId, string title);
        Task<ChatSessionDetailsDto> GetChatDetailsAsync(Guid conversationId);
        Task SetupConversationTeamAsync(Guid conversationId, Guid teamId);
        Task<ChatMessageDto> ProcessUserMessageAsync(Guid conversationId, string content, string senderName);
        Task DeleteConversationAsync(Guid conversationId);
        Task DeleteMessageAsync(Guid messageId);
    }
}
