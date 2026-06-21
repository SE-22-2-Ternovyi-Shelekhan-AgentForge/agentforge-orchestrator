using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using AgentForge.Orchestrator.Messaging;
using AgentForge.Orchestrator.Exceptions;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Models.Broker;
using AgentForge.Orchestrator.Services;
using AgentForge.Orchestrator.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgentForge.Orchestrator.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IConversationRepository> _convRepoMock;
        private readonly Mock<IChatRepository> _chatRepoMock;
        private readonly Mock<IAgentTeamRepository> _teamRepoMock;
        private readonly Mock<IAgentSessionTraceRepository> _traceRepoMock;
        private readonly Mock<ISessionPublisher> _publisherMock;
        private readonly IMapper _mapper;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _convRepoMock  = new Mock<IConversationRepository>();
            _chatRepoMock  = new Mock<IChatRepository>();
            _teamRepoMock  = new Mock<IAgentTeamRepository>();
            _traceRepoMock = new Mock<IAgentSessionTraceRepository>();
            _publisherMock = new Mock<ISessionPublisher>();
            var loggerFactory = new NullLoggerFactory();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrchestratorProfile>();
            }, loggerFactory);

            _mapper = configuration.CreateMapper();

            _chatService = new ChatService(
                _convRepoMock.Object,
                _chatRepoMock.Object,
                _teamRepoMock.Object,
                _traceRepoMock.Object,
                _mapper,
                _publisherMock.Object);
        }

        [Fact]
        public async Task GetUserChatsAsync_ReturnsMappedChats()
        {
            var userId = Guid.NewGuid();
            var team = new AgentTeam { AgentTeamId = Guid.NewGuid(), Name = "Support Team" };
            var chats = new List<Conversation>
            {
                new Conversation { ConversationId = Guid.NewGuid(), UserId = userId, Title = "Chat 1", Team = team },
                new Conversation { ConversationId = Guid.NewGuid(), UserId = userId, Title = "Chat 2" }
            };

            _convRepoMock.Setup(r => r.RetrieveByUserIdAsync(userId)).ReturnsAsync(chats);

            var result = await _chatService.GetUserChatsAsync(userId);

            result.Should().HaveCount(2);
            result.First().TeamName.Should().Be("Support Team");
            result.Last().TeamName.Should().BeNull();
        }

        [Fact]
        public async Task GetChatDetailsAsync_Success_ReturnsDetailsWithMessages()
        {
            var convId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = new Conversation { ConversationId = convId, UserId = userId, Title = "Test Details" };
            var history = new List<ChatMessage>
            {
                new ChatMessage { ChatMessageId = Guid.NewGuid(), ConversationId = convId, Content = "Hello" }
            };

            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(conversation);
            _chatRepoMock.Setup(r => r.RetrieveHistoryAsync(convId)).ReturnsAsync(history);

            var result = await _chatService.GetChatDetailsAsync(convId, userId);

            result.Should().NotBeNull();
            result.Title.Should().Be("Test Details");
            result.Messages.Should().HaveCount(1);
            result.Messages.First().Content.Should().Be("Hello");
        }

        [Fact]
        public async Task GetChatDetailsAsync_WhenNotOwner_ThrowsForbiddenAccessException()
        {
            var convId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var conversation = new Conversation { ConversationId = convId, UserId = ownerId, Title = "Someone else's chat" };

            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(conversation);

            Func<Task> action = async () => await _chatService.GetChatDetailsAsync(convId, otherUserId);

            await action.Should().ThrowAsync<ForbiddenAccessException>();
        }

        [Fact]
        public async Task SetupConversationTeamAsync_TeamNotFound_ThrowsKeyNotFoundException()
        {
            var convId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(new Conversation { ConversationId = convId, UserId = userId });
            _teamRepoMock.Setup(r => r.RetrieveAsync(teamId)).ReturnsAsync((AgentTeam?)null);

            Func<Task> action = async () => await _chatService.SetupConversationTeamAsync(convId, teamId, userId);

            await action.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Team {teamId} not found.");
        }

        [Fact]
        public async Task ProcessUserMessageAsync_Success_CreatesAndReturnsMessage()
        {
            var convId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = new Conversation { ConversationId = convId, UserId = userId, AgentTeamId = teamId };
            var team = new AgentTeam
            {
                AgentTeamId = teamId,
                Agents = new List<Agent> { new Agent { Role = "developer" } }
            };

            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(conversation);
            _teamRepoMock.Setup(r => r.RetrieveAsync(teamId)).ReturnsAsync(team);
            _chatRepoMock.Setup(r => r.RetrieveHistoryAsync(convId)).ReturnsAsync(new List<ChatMessage>());

            var result = await _chatService.ProcessUserMessageAsync(convId, "Hi bot", "User1", userId);

            result.Should().NotBeNull();
            result.SessionId.Should().NotBeEmpty();
            result.Message.Content.Should().Be("Hi bot");
            result.Message.Role.Should().Be("user");

            _chatRepoMock.Verify(r => r.AddMessageAsync(It.Is<ChatMessage>(m =>
                m.ConversationId == convId &&
                m.Content == "Hi bot")), Times.Once);

            _publisherMock.Verify(p => p.PublishSession(It.Is<AgentSessionRequestedDto>(s =>
                s.ConversationId == convId &&
                s.UserPrompt == "Hi bot")), Times.Once);
        }

        [Fact]
        public async Task ProcessUserMessageAsync_NoTeamDesignated_ThrowsInvalidOperationException()
        {
            var convId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = new Conversation { ConversationId = convId, UserId = userId, AgentTeamId = null };

            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(conversation);

            Func<Task> action = async () => await _chatService.ProcessUserMessageAsync(convId, "Test", "User", userId);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot send message to a conversation without a designated team.");
        }

        [Fact]
        public async Task GetChatDetailsAsync_ConversationExistsButNoTeam_ReturnsDetailsWithEmptyAgents()
        {
            var convId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = new Conversation { ConversationId = convId, UserId = userId, Title = "No Team Chat", AgentTeamId = null };

            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(conversation);
            _chatRepoMock.Setup(r => r.RetrieveHistoryAsync(convId)).ReturnsAsync(new List<ChatMessage>());

            var result = await _chatService.GetChatDetailsAsync(convId, userId);

            result.TeamId.Should().BeNull();
            result.Agents.Should().BeEmpty();
        }

        [Fact]
        public async Task ProcessUserMessageAsync_WhenTeamHasNoAgents_ThrowsEmptyTeamException()
        {
            var convId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = new Conversation { ConversationId = convId, UserId = userId, AgentTeamId = teamId };
            var team = new AgentTeam { AgentTeamId = teamId, Agents = new List<Agent>() };

            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(conversation);
            _teamRepoMock.Setup(r => r.RetrieveAsync(teamId)).ReturnsAsync(team);

            Func<Task> action = async () => await _chatService.ProcessUserMessageAsync(convId, "Hello", "User", userId);

            await action.Should().ThrowAsync<EmptyTeamException>()
                .WithMessage("*team has no agents*");
        }

        [Fact]
        public async Task DeleteConversationAsync_WhenOwner_CallsRepositoryDelete()
        {
            var convId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _convRepoMock.Setup(r => r.RetrieveAsync(convId))
                .ReturnsAsync(new Conversation { ConversationId = convId, UserId = userId });

            await _chatService.DeleteConversationAsync(convId, userId);

            _convRepoMock.Verify(r => r.DeleteAsync(convId), Times.Once);
        }

        [Fact]
        public async Task DeleteConversationAsync_WhenNotOwner_ThrowsAndDoesNotDelete()
        {
            var convId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            _convRepoMock.Setup(r => r.RetrieveAsync(convId))
                .ReturnsAsync(new Conversation { ConversationId = convId, UserId = ownerId });

            Func<Task> action = async () => await _chatService.DeleteConversationAsync(convId, otherUserId);

            await action.Should().ThrowAsync<ForbiddenAccessException>();
            _convRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMessageAsync_WhenOwner_CallsRepositoryDelete()
        {
            var messageId = Guid.NewGuid();
            var convId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _chatRepoMock.Setup(r => r.RetrieveByIdAsync(messageId))
                .ReturnsAsync(new ChatMessage { ChatMessageId = messageId, ConversationId = convId });
            _convRepoMock.Setup(r => r.RetrieveAsync(convId))
                .ReturnsAsync(new Conversation { ConversationId = convId, UserId = userId });

            await _chatService.DeleteMessageAsync(messageId, userId);

            _chatRepoMock.Verify(r => r.DeleteAsync(messageId), Times.Once);
        }

        [Fact]
        public async Task SetupConversationTeamAsync_Success_UpdatesConversation()
        {
            var convId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = new Conversation { ConversationId = convId, UserId = userId };
            var team = new AgentTeam { AgentTeamId = teamId };

            _convRepoMock.Setup(r => r.RetrieveAsync(convId)).ReturnsAsync(conversation);
            _teamRepoMock.Setup(r => r.RetrieveAsync(teamId)).ReturnsAsync(team);

            await _chatService.SetupConversationTeamAsync(convId, teamId, userId);

            conversation.AgentTeamId.Should().Be(teamId);
            _convRepoMock.Verify(r => r.UpdateAsync(conversation), Times.Once);
        }
    }
}