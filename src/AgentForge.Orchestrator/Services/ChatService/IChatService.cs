using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Models.Broker;

namespace AgentForge.Orchestrator.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ConversationDto>> GetUserChatsAsync(Guid userId);
        Task<Guid> CreateEmptyChatAsync(Guid userId, string title);
        Task<ChatSessionDetailsDto> GetChatDetailsAsync(Guid conversationId, Guid userId);
        Task SetupConversationTeamAsync(Guid conversationId, Guid teamId, Guid userId);
        Task RenameConversationAsync(Guid conversationId, string title, Guid userId);
        Task<SendMessageResponse> ProcessUserMessageAsync(Guid conversationId, string content, string senderName, Guid userId);
        Task DeleteConversationAsync(Guid conversationId, Guid userId);
        Task DeleteMessageAsync(Guid messageId, Guid userId);
        Task<AgentSessionTraceDto> GetSessionTraceAsync(Guid sessionId, Guid userId);
    }
}
