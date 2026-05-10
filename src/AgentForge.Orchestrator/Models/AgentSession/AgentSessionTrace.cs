using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgentForge.Orchestrator.Models;

public class AgentSessionTrace
{
    [Key]
    public Guid SessionId { get; set; }

    [Required]
    public Guid ConversationId { get; set; }

    [ForeignKey("ConversationId")]
    public Conversation Conversation { get; set; } = null!;

    public List<TraceEntryRecord> Trace { get; set; } = new();

    public int? TokensInTotal { get; set; }
    public int? TokensOutTotal { get; set; }
    public int Iterations { get; set; }
    public DateTime CompletedAt { get; set; }

    public string? ErrorType { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TraceEntryRecord
{
    public string AgentRole { get; set; } = "";
    public string Output { get; set; } = "";
    public List<string> ToolsUsed { get; set; } = new();
    public int? TokensIn { get; set; }
    public int? TokensOut { get; set; }
}
