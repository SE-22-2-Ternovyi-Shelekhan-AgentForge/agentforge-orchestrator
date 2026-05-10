using System.Text.Json;
using AgentForge.Orchestrator.Hubs;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Models.Broker;
using AgentForge.Orchestrator.Repositories;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgentForge.Orchestrator.Messaging;

public class ErrorsConsumer : BackgroundService
{
    private readonly IRabbitMqConnection _conn;
    private readonly IServiceScopeFactory _scopes;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IConfiguration _cfg;
    private readonly ILogger<ErrorsConsumer> _log;
    private IModel? _channel;

    public ErrorsConsumer(
        IRabbitMqConnection conn,
        IServiceScopeFactory scopes,
        IHubContext<ChatHub> hub,
        IConfiguration cfg,
        ILogger<ErrorsConsumer> log)
    {
        _conn   = conn;
        _scopes = scopes;
        _hub    = hub;
        _cfg    = cfg;
        _log    = log;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _conn.CreateChannel();
        var queue = _cfg["RabbitMQ:ErrorsQueue"] ?? "agent-errors-queue";
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.BasicQos(0, prefetchCount: 4, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) => await OnError(ea, stoppingToken);
        _channel.BasicConsume(queue, autoAck: false, consumer);

        _log.LogInformation("ErrorsConsumer started, listening on {Queue}", queue);
        stoppingToken.Register(() => _channel?.Close());
        return Task.CompletedTask;
    }

    private async Task OnError(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<AgentSessionFailedDto>(ea.Body.Span)
                      ?? throw new InvalidOperationException("null payload");

            // parse_error sentinel: worker couldn't parse the incoming message
            if (evt.SessionId == Guid.Empty)
            {
                _log.LogError("Received parse_error from worker: {Msg}", evt.ErrorMessage);
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                return;
            }

            using var scope = _scopes.CreateScope();
            var chatRepo    = scope.ServiceProvider.GetRequiredService<IChatRepository>();

            var msg = new ChatMessage
            {
                ConversationId = evt.ConversationId,
                Content        = $"[{evt.ErrorType}] {evt.ErrorMessage}",
                Role           = "system",
                SenderName     = "system",
                Timestamp      = DateTime.UtcNow,
                AgentSessionId = evt.SessionId,
            };
            await chatRepo.AddMessageAsync(msg);

            await _hub.Clients
                .Group(evt.ConversationId.ToString())
                .SendAsync("SessionFailed", new
                {
                    sessionId    = evt.SessionId,
                    errorType    = evt.ErrorType,
                    errorMessage = evt.ErrorMessage,
                }, ct);

            _channel!.BasicAck(ea.DeliveryTag, multiple: false);

            _log.LogError("Session {SessionId} failed with {ErrorType}: {ErrorMessage}",
                evt.SessionId, evt.ErrorType, evt.ErrorMessage);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ErrorsConsumer: failed to process message");
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
