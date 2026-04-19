using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Repositories;
using AutoMapper;

namespace AgentForge.Orchestrator.Services
{
    public class AgentService : IAgentService
    {
        private readonly IAgentTeamRepository _teamRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IMapper _mapper;

        public AgentService(IAgentTeamRepository teamRepo, IAgentRepository agentRepo, IMapper mapper)
        {
            _teamRepository = teamRepo;
            _agentRepository = agentRepo;
            _mapper = mapper;
        }

        public async Task<AgentTeamDto> CreateTeamAsync(string name)
        {
            var team = new AgentTeam { Name = name };
            await _teamRepository.CreateAsync(team);

            return _mapper.Map<AgentTeamDto>(team);
        }

        public async Task<AgentDto> CreateAgentAsync(AgentDto agentDto)
        {
            var agent = _mapper.Map<Agent>(agentDto);
            await _agentRepository.CreateAsync(agent);

            return _mapper.Map<AgentDto>(agent);
        }

        public async Task<IEnumerable<AgentTeamDto>> GetAllTeamsAsync()
        {
            var teams = await _teamRepository.RetrieveAsync();
            return _mapper.Map<IEnumerable<AgentTeamDto>>(teams);
        }

        public async Task UpdateAgentAsync(AgentDto agentDto)
        {
            var existingAgent = await _agentRepository.RetrieveByIdAsync(agentDto.AgentId);
            if (existingAgent == null)
            {
                throw new KeyNotFoundException("Agent not found");
            }

            _mapper.Map(agentDto, existingAgent);
            await _agentRepository.UpdateAsync(existingAgent);
        }

        public async Task DeleteAgentAsync(Guid agentId)
        {
            await _agentRepository.DeleteAsync(agentId);
        }

        public async Task UpdateTeamAsync(AgentTeamDto agentTeamDto)
        {
            var existingAgentTeam = await _teamRepository.RetrieveAsync(agentTeamDto.AgentTeamId);
            if (existingAgentTeam == null)
            {
                throw new KeyNotFoundException("Agent not found");
            }

            _mapper.Map(agentTeamDto, existingAgentTeam);
            await _teamRepository.UpdateAsync(existingAgentTeam);
        }
    }
}