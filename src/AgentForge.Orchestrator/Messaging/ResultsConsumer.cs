using System.Text.Json;
using AgentForge.Orchestrator.Hubs;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Models.Broker;
using AgentForge.Orchestrator.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgentForge.Orchestrator.Messaging;

public class ResultsConsumer : BackgroundService
{
    private readonly IRabbitMqConnection _conn;
    private readonly IServiceScopeFactory _scopes;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IConfiguration _cfg;
    private readonly IMapper _mapper;
    private readonly ILogger<ResultsConsumer> _log;
    private IModel? _channel;

    public ResultsConsumer(
        IRabbitMqConnection conn,
        IServiceScopeFactory scopes,
        IHubContext<ChatHub> hub,
        IConfiguration cfg,
        IMapper mapper,
        ILogger<ResultsConsumer> log)
    {
        _conn   = conn;
        _scopes = scopes;
        _hub    = hub;
        _cfg    = cfg;
        _mapper = mapper;
        _log    = log;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _conn.CreateChannel();
        var queue = _cfg["RabbitMQ:ResultsQueue"] ?? "agent-results-queue";
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.BasicQos(0, prefetchCount: 4, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) => await OnResult(ea, stoppingToken);
        _channel.BasicConsume(queue, autoAck: false, consumer);

        _log.LogInformation("ResultsConsumer started, listening on {Queue}", queue);
        stoppingToken.Register(() => _channel?.Close());
        return Task.CompletedTask;
    }

    private async Task OnResult(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<AgentSessionCompletedDto>(ea.Body.Span)
                      ?? throw new InvalidOperationException("null payload");

            using var scope    = _scopes.CreateScope();
            var chatRepo       = scope.ServiceProvider.GetRequiredService<IChatRepository>();
            var traceRepo      = scope.ServiceProvider.GetRequiredService<IAgentSessionTraceRepository>();

            var msg = new ChatMessage
            {
                ConversationId = evt.ConversationId,
                Content        = evt.FinalOutput,
                Role           = "assistant",
                // The final reply is the team's synthesized summary (the
                // per-agent contributions live in the session trace).
                SenderName     = "summary",
                Timestamp      = DateTime.UtcNow,
                AgentSessionId = evt.SessionId,
            };
            await chatRepo.AddMessageAsync(msg);

            await traceRepo.AddAsync(new AgentSessionTrace
            {
                SessionId      = evt.SessionId,
                ConversationId = evt.ConversationId,
                Trace          = evt.Trace.Select(t => new TraceEntryRecord
                {
                    AgentRole  = t.AgentRole,
                    Output     = t.Output,
                    ToolsUsed  = t.ToolsUsed,
                    TokensIn   = t.TokensIn,
                    TokensOut  = t.TokensOut,
                }).ToList(),
                TokensInTotal  = evt.TokensInTotal,
                TokensOutTotal = evt.TokensOutTotal,
                Iterations     = evt.Iterations,
                CompletedAt    = evt.CompletedAt,
            });

            await _hub.Clients
                .Group(evt.ConversationId.ToString())
                .SendAsync("MessageAppended", _mapper.Map<ChatMessageDto>(msg), ct);

            _channel!.BasicAck(ea.DeliveryTag, multiple: false);

            _log.LogInformation("Session {SessionId} completed, {Iter} iterations, saved to conversation {ConvId}",
                evt.SessionId, evt.Iterations, evt.ConversationId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ResultsConsumer: failed to process message");
            _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}
