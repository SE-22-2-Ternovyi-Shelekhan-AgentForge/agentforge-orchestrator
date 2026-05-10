using Microsoft.AspNetCore.SignalR;

namespace AgentForge.Orchestrator.Hubs;

public class ChatHub : Hub
{
    public Task JoinConversation(Guid conversationId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());

    public Task LeaveConversation(Guid conversationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
}
