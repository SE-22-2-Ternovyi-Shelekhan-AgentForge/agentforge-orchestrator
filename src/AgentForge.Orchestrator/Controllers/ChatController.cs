using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Orchestrator.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserChats(Guid userId)
        {
            var result = await _chatService.GetUserChatsAsync(userId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateConversationRequest request)
        {
            var newId = await _chatService.CreateEmptyChatAsync(request.UserId, request.Title);
            return Ok(newId);
        }

        [HttpPost("setup")]
        public async Task<IActionResult> SetupTeam([FromBody] SetupTeamRequest setupTeamRequest)
        {
            try
            {
                await _chatService.SetupConversationTeamAsync(setupTeamRequest.ConversationId, setupTeamRequest.TeamId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{conversationId}/details")]
        public async Task<IActionResult> GetDetails(Guid conversationId)
        {
            try
            {
                var result = await _chatService.GetChatDetailsAsync(conversationId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var result = await _chatService.ProcessUserMessageAsync(
                    request.ConversationId,
                    request.Content,
                    request.SenderName);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> DeleteConversation(Guid conversationId)
        {
            try
            {
                await _chatService.DeleteConversationAsync(conversationId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}