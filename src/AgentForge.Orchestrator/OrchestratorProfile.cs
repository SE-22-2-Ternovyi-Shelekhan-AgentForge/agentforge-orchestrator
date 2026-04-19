using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Models.Broker;
using AutoMapper;

namespace AgentForge.Orchestrator
{
    public class OrchestratorProfile : Profile
    {
        public OrchestratorProfile()
        {
            CreateMap<Agent, AgentDto>()
                .ForMember(dest => dest.Capabilities, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Capabilities)
                        ? new List<string>()
                        : src.Capabilities.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()))
                .ReverseMap()
                .ForMember(dest => dest.Capabilities, opt => opt.MapFrom(src =>
                    src.Capabilities != null
                        ? string.Join(",", src.Capabilities)
                        : string.Empty));

            CreateMap<AgentTeam, AgentTeamDto>().ReverseMap();

            CreateMap<ChatMessage, ChatMessageDto>().ReverseMap();

            CreateMap<Conversation, ConversationDto>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null));

            CreateMap<Conversation, ChatSessionDetailsDto>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null))
                .ForMember(dest => dest.Agents, opt => opt.MapFrom(src => src.Team != null ? src.Team.Agents : new List<Agent>()))
                .ForMember(dest => dest.Messages, opt => opt.Ignore());
        }
    }
}