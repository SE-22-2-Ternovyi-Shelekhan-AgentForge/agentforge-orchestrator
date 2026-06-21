using System.Security.Claims;
using AgentForge.Orchestrator.Exceptions;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Orchestrator.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue("sub")
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Authenticated user has no subject claim."));

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyChats()
        {
            var result = await _chatService.GetUserChatsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateConversationRequest request)
        {
            var newId = await _chatService.CreateEmptyChatAsync(CurrentUserId, request.Title);
            return Ok(newId);
        }

        [HttpPost("setup")]
        public async Task<IActionResult> SetupTeam([FromBody] SetupTeamRequest setupTeamRequest)
        {
            try
            {
                await _chatService.SetupConversationTeamAsync(setupTeamRequest.ConversationId, setupTeamRequest.TeamId, CurrentUserId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        [HttpPut("{conversationId}/rename")]
        public async Task<IActionResult> RenameConversation(Guid conversationId, [FromBody] RenameConversationRequest request)
        {
            try
            {
                await _chatService.RenameConversationAsync(conversationId, request.Title, CurrentUserId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        [HttpGet("{conversationId}/details")]
        public async Task<IActionResult> GetDetails(Guid conversationId)
        {
            try
            {
                var result = await _chatService.GetChatDetailsAsync(conversationId, CurrentUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
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
                    request.SenderName,
                    CurrentUserId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (EmptyTeamException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("trace/{sessionId}")]
        public async Task<IActionResult> GetSessionTrace(Guid sessionId)
        {
            try
            {
                var result = await _chatService.GetSessionTraceAsync(sessionId, CurrentUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        [HttpDelete("message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            try
            {
                await _chatService.DeleteMessageAsync(messageId, CurrentUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> DeleteConversation(Guid conversationId)
        {
            try
            {
                await _chatService.DeleteConversationAsync(conversationId, CurrentUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }
    }
}