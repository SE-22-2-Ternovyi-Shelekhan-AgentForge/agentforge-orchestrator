using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AgentForge.Orchestrator.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public Task JoinConversation(Guid conversationId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());

    public Task LeaveConversation(Guid conversationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
}
