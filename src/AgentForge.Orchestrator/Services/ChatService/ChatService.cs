using AgentForge.Orchestrator.Messaging;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Models.Broker;
using AgentForge.Orchestrator.Repositories;
using AutoMapper;

namespace AgentForge.Orchestrator.Services
{
    public class ChatService : IChatService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IAgentTeamRepository _teamRepository;
        private readonly IMapper _mapper;
        private readonly ISessionPublisher _sessionPublisher;

        public ChatService(
            IConversationRepository convRepo,
            IChatRepository chatRepo,
            IAgentTeamRepository teamRepo,
            IMapper mapper,
            ISessionPublisher sessionPublisher)
        {
            _conversationRepository = convRepo;
            _chatRepository = chatRepo;
            _teamRepository = teamRepo;
            _mapper = mapper;
            _sessionPublisher = sessionPublisher;
        }

        public async Task<IEnumerable<ConversationDto>> GetUserChatsAsync(Guid userId)
        {
            var chats = await _conversationRepository.RetrieveByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ConversationDto>>(chats);
        }

        public async Task<Guid> CreateEmptyChatAsync(Guid userId, string title)
        {
            var conversation = new Conversation
            {
                UserId = userId,
                Title = title
            };

            await _conversationRepository.CreateAsync(conversation);
            return conversation.ConversationId;
        }

        public async Task<ChatSessionDetailsDto> GetChatDetailsAsync(Guid conversationId)
        {
            var conv = await _conversationRepository.RetrieveAsync(conversationId);
            if (conv == null)
            {
                throw new KeyNotFoundException($"Conversation {conversationId} not found.");
            }

            var detailsDto = _mapper.Map<ChatSessionDetailsDto>(conv);

            var history = await _chatRepository.RetrieveHistoryAsync(conversationId);
            detailsDto.Messages = _mapper.Map<List<ChatMessageDto>>(history);

            return detailsDto;
        }

        public async Task SetupConversationTeamAsync(Guid conversationId, Guid teamId)
        {
            var conv = await _conversationRepository.RetrieveAsync(conversationId);
            if (conv == null)
            {
                throw new KeyNotFoundException($"Conversation {conversationId} not found.");
            }

            var team = await _teamRepository.RetrieveAsync(teamId);
            if (team == null)
            {
                throw new KeyNotFoundException($"Team {teamId} not found.");
            }

            conv.AgentTeamId = teamId;
            await _conversationRepository.UpdateAsync(conv);
        }

        public async Task<ChatMessageDto> ProcessUserMessageAsync(Guid conversationId, string content, string senderName)
        {
            var conversation = await _conversationRepository.RetrieveAsync(conversationId);
            if (conversation == null)
                throw new KeyNotFoundException($"Conversation {conversationId} not found.");

            if (conversation.AgentTeamId == null)
                throw new InvalidOperationException("Cannot send message to a conversation without a designated team.");

            // 1) Persist user message
            var message = new ChatMessage
            {
                ConversationId = conversationId,
                Content        = content,
                Role           = "user",
                SenderName     = senderName,
                Timestamp      = DateTime.UtcNow,
            };
            await _chatRepository.AddMessageAsync(message);

            // 2) Load team with agents
            var team = await _teamRepository.RetrieveAsync(conversation.AgentTeamId.Value)
                ?? throw new KeyNotFoundException($"Team {conversation.AgentTeamId} not found.");

            // 3) Build context from history (last 20 messages, excluding the just-saved one)
            var history = (await _chatRepository.RetrieveHistoryAsync(conversationId))
                .Where(m => m.ChatMessageId != message.ChatMessageId
                         && (m.Role == "user" || m.Role == "assistant"))
                .OrderByDescending(m => m.Timestamp)
                .Take(20)
                .OrderBy(m => m.Timestamp)
                .Select(m => new ContextMessageDto
                {
                    Role      = m.Role,
                    Content   = m.Content,
                    AgentRole = m.Role == "assistant" ? m.SenderName : null,
                    Timestamp = m.Timestamp,
                })
                .ToList();

            // 4) Publish session to worker via RabbitMQ
            _sessionPublisher.PublishSession(new AgentSessionRequestedDto
            {
                SessionId      = Guid.NewGuid(),
                ConversationId = conversationId,
                UserPrompt     = content,
                History        = history,
                Team = new TeamConfigDto
                {
                    SupervisorPrompt = team.SupervisorPrompt,
                    MaxIterations    = 10,
                    Agents           = team.Agents.Select(a => new AgentConfigDto
                    {
                        Role         = a.Role,
                        SystemPrompt = a.SystemPrompt,
                        Model        = a.ModelName,
                        Temperature  = a.Temperature,
                        Tools        = a.Tools,
                    }).ToList(),
                },
            });

            // 5) Return user message immediately — agent response arrives via SignalR
            return _mapper.Map<ChatMessageDto>(message);
        }

        public async Task DeleteConversationAsync(Guid conversationId)
        {
            await _conversationRepository.DeleteAsync(conversationId);
        }

        public async Task DeleteMessageAsync(Guid messageId)
        {
            await _chatRepository.DeleteAsync(messageId);
        }
    }
}