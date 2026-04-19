using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Orchestrator.Controllers
{
    [ApiController]
    [Route("api/agents")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly IChatService _chatService;

        public AgentController(IAgentService agentService, IChatService chatService)
        {
            _agentService = agentService;
            _chatService = chatService;
        }

        [HttpGet("teams")]
        public async Task<IActionResult> GetAllTeams()
        {
            var result = await _agentService.GetAllTeamsAsync();
            return Ok(result);
        }

        [HttpPost("teams")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Name is required");
            }

            var team = await _agentService.CreateTeamAsync(request.Name);

            if (request.ConversationId != Guid.Empty)
            {
                await _chatService.SetupConversationTeamAsync(request.ConversationId, team.AgentTeamId);
            }

            return Ok(team);
        }

        [HttpPut("teams/{teamId}")]
        public async Task<IActionResult> UpdateTeam([FromBody] AgentTeamDto agentTeam)
        {
            try
            {
                await _agentService.UpdateTeamAsync(agentTeam);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAgent([FromBody] AgentDto request)
        {
            var result = await _agentService.CreateAgentAsync(request);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAgent([FromBody] AgentDto request)
        {
            try
            {
                await _agentService.UpdateAgentAsync(request);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{agentId}")]
        public async Task<IActionResult> DeleteAgent(Guid agentId)
        {
            try
            {
                await _agentService.DeleteAgentAsync(agentId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}