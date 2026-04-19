using Moq;
using FluentAssertions;
using AutoMapper;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Services;
using AgentForge.Orchestrator.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentForge.Orchestrator.Tests.Services
{
    public class AgentServiceTests
    {
        private readonly Mock<IAgentTeamRepository> _teamRepoMock;
        private readonly Mock<IAgentRepository> _agentRepoMock;
        private readonly IMapper _mapper;
        private readonly AgentService _agentService;

        public AgentServiceTests()
        {
            _teamRepoMock = new Mock<IAgentTeamRepository>();
            _agentRepoMock = new Mock<IAgentRepository>();

            var loggerFactory = new NullLoggerFactory();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<OrchestratorProfile>(), loggerFactory);
            _mapper = configuration.CreateMapper();

            _agentService = new AgentService(
                _teamRepoMock.Object,
                _agentRepoMock.Object,
                _mapper);
        }

        [Fact]
        public async Task CreateTeamAsync_Success_SavesAndReturnsMappedDto()
        {
            var teamName = "Backend Developers";

            var result = await _agentService.CreateTeamAsync(teamName);

            result.Should().NotBeNull();
            result.Name.Should().Be(teamName);

            _teamRepoMock.Verify(r => r.CreateAsync(It.Is<AgentTeam>(t => t.Name == teamName)), Times.Once);
        }

        [Fact]
        public async Task CreateAgentAsync_Success_SavesAndReturnsMappedDto()
        {
            var teamId = Guid.NewGuid();
            var agentDto = new AgentDto
            {
                AgentTeamId = teamId,
                Name = "SQL Expert",
                Role = "Developer",
                ModelName = "gpt-4",
                SystemPrompt = "You write SQL"
            };

            var result = await _agentService.CreateAgentAsync(agentDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("SQL Expert");

            _agentRepoMock.Verify(r => r.CreateAsync(It.Is<Agent>(a =>
                a.AgentTeamId == teamId &&
                a.Name == "SQL Expert" &&
                a.ModelName == "gpt-4")), Times.Once);
        }

        [Fact]
        public async Task GetAllTeamsAsync_ReturnsMappedCollection()
        {
            var teams = new List<AgentTeam>
            {
                new AgentTeam
                {
                    AgentTeamId = Guid.NewGuid(),
                    Name = "Team A",
                    Agents = new List<Agent> { new Agent { Name = "Bot 1" } }
                },
                new AgentTeam { AgentTeamId = Guid.NewGuid(), Name = "Team B" }
            };

            _teamRepoMock.Setup(r => r.RetrieveAsync()).ReturnsAsync(teams);

            var result = await _agentService.GetAllTeamsAsync();

            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Team A");
        }

        [Fact]
        public async Task UpdateAgentAsync_Success_CallsRepositoryUpdate()
        {
            var agentId = Guid.NewGuid();
            var agentDto = new AgentDto { AgentId = agentId, Name = "Updated Name" };
            var existingAgent = new Agent { AgentId = agentId, Name = "Old Name" };

            _agentRepoMock.Setup(r => r.RetrieveByIdAsync(agentId)).ReturnsAsync(existingAgent);

            await _agentService.UpdateAgentAsync(agentDto);

            existingAgent.Name.Should().Be("Updated Name");
            _agentRepoMock.Verify(r => r.UpdateAsync(It.Is<Agent>(a => a.AgentId == agentId)), Times.Once);
        }

        [Fact]
        public async Task UpdateTeamAsync_Success_CallsRepositoryUpdate()
        {
            var teamId = Guid.NewGuid();
            var teamDto = new AgentTeamDto { AgentTeamId = teamId, Name = "New Team Name" };
            var existingTeam = new AgentTeam { AgentTeamId = teamId, Name = "Old Team Name" };

            _teamRepoMock.Setup(r => r.RetrieveAsync(teamId)).ReturnsAsync(existingTeam);

            await _agentService.UpdateTeamAsync(teamDto);

            existingTeam.Name.Should().Be("New Team Name");
            _teamRepoMock.Verify(r => r.UpdateAsync(It.Is<AgentTeam>(t => t.AgentTeamId == teamId)), Times.Once);
        }

        [Fact]
        public async Task DeleteAgentAsync_CallsRepositoryDelete()
        {
            var agentId = Guid.NewGuid();

            await _agentService.DeleteAgentAsync(agentId);

            _agentRepoMock.Verify(r => r.DeleteAsync(agentId), Times.Once);
        }

        [Fact]
        public async Task UpdateAgentAsync_PartialUpdate_OnlyChangesProvidedFields()
        {
            var agentId = Guid.NewGuid();
            var existingAgent = new Agent { AgentId = agentId, Name = "Old Name", SystemPrompt = "Old Prompt" };
            var updateDto = new AgentDto { AgentId = agentId, Name = "New Name", SystemPrompt = "New Prompt" };

            _agentRepoMock.Setup(r => r.RetrieveByIdAsync(agentId)).ReturnsAsync(existingAgent);

            await _agentService.UpdateAgentAsync(updateDto);

            existingAgent.Name.Should().Be("New Name");
            existingAgent.SystemPrompt.Should().Be("New Prompt");
            _agentRepoMock.Verify(r => r.UpdateAsync(existingAgent), Times.Once);
        }

        [Fact]
        public async Task UpdateAgentAsync_WithCapabilities_CorrectlySerializesToString()
        {
            var agentId = Guid.NewGuid();
            var existingAgent = new Agent { AgentId = agentId, Capabilities = "" };
            var updateDto = new AgentDto { AgentId = agentId, Capabilities = new List<string> { "web", "tools" } };

            _agentRepoMock.Setup(r => r.RetrieveByIdAsync(agentId)).ReturnsAsync(existingAgent);

            await _agentService.UpdateAgentAsync(updateDto);

            existingAgent.Capabilities.Should().Be("web,tools");
        }
    }
}