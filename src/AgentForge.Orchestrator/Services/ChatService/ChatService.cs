using AgentForge.Orchestrator.Exceptions;
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
        private readonly IAgentSessionTraceRepository _traceRepository;
        private readonly IMapper _mapper;
        private readonly ISessionPublisher _sessionPublisher;

        public ChatService(
            IConversationRepository convRepo,
            IChatRepository chatRepo,
            IAgentTeamRepository teamRepo,
            IAgentSessionTraceRepository traceRepo,
            IMapper mapper,
            ISessionPublisher sessionPublisher)
        {
            _conversationRepository = convRepo;
            _chatRepository = chatRepo;
            _teamRepository = teamRepo;
            _traceRepository = traceRepo;
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

        public async Task<ChatSessionDetailsDto> GetChatDetailsAsync(Guid conversationId, Guid userId)
        {
            var conv = await GetOwnedConversationAsync(conversationId, userId);

            var detailsDto = _mapper.Map<ChatSessionDetailsDto>(conv);

            var history = await _chatRepository.RetrieveHistoryAsync(conversationId);
            detailsDto.Messages = _mapper.Map<List<ChatMessageDto>>(history);

            return detailsDto;
        }

        public async Task SetupConversationTeamAsync(Guid conversationId, Guid teamId, Guid userId)
        {
            var conv = await GetOwnedConversationAsync(conversationId, userId);

            var team = await _teamRepository.RetrieveAsync(teamId);
            if (team == null)
            {
                throw new KeyNotFoundException($"Team {teamId} not found.");
            }

            conv.AgentTeamId = teamId;
            await _conversationRepository.UpdateAsync(conv);
        }

        public async Task RenameConversationAsync(Guid conversationId, string title, Guid userId)
        {
            var conv = await GetOwnedConversationAsync(conversationId, userId);

            conv.Title = title;
            await _conversationRepository.UpdateAsync(conv);
        }

        public async Task<SendMessageResponse> ProcessUserMessageAsync(Guid conversationId, string content, string senderName, Guid userId)
        {
            var conversation = await GetOwnedConversationAsync(conversationId, userId);

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

            if (team.Agents.Count == 0)
                throw new EmptyTeamException("Cannot process message: the team has no agents configured.");

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
            var sessionId = Guid.NewGuid();
            _sessionPublisher.PublishSession(new AgentSessionRequestedDto
            {
                SessionId      = sessionId,
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

            // 5) Return user message + session id immediately — agent response arrives via SignalR
            return new SendMessageResponse
            {
                Message   = _mapper.Map<ChatMessageDto>(message),
                SessionId = sessionId,
            };
        }

        public async Task DeleteConversationAsync(Guid conversationId, Guid userId)
        {
            await GetOwnedConversationAsync(conversationId, userId);
            await _conversationRepository.DeleteAsync(conversationId);
        }

        public async Task DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _chatRepository.RetrieveByIdAsync(messageId)
                ?? throw new KeyNotFoundException($"Message {messageId} not found.");

            await GetOwnedConversationAsync(message.ConversationId, userId);
            await _chatRepository.DeleteAsync(messageId);
        }

        public async Task<AgentSessionTraceDto> GetSessionTraceAsync(Guid sessionId, Guid userId)
        {
            var trace = await _traceRepository.RetrieveBySessionAsync(sessionId)
                ?? throw new KeyNotFoundException($"Trace for session {sessionId} not found.");

            await GetOwnedConversationAsync(trace.ConversationId, userId);

            return _mapper.Map<AgentSessionTraceDto>(trace);
        }

        /// <summary>
        /// Retrieves a conversation and verifies it belongs to the given user.
        /// Throws <see cref="KeyNotFoundException"/> if it does not exist, or
        /// <see cref="ForbiddenAccessException"/> if it belongs to another user.
        /// </summary>
        private async Task<Conversation> GetOwnedConversationAsync(Guid conversationId, Guid userId)
        {
            var conversation = await _conversationRepository.RetrieveAsync(conversationId)
                ?? throw new KeyNotFoundException($"Conversation {conversationId} not found.");

            if (conversation.UserId != userId)
                throw new ForbiddenAccessException($"Conversation {conversationId} does not belong to the current user.");

            return conversation;
        }
    }
}