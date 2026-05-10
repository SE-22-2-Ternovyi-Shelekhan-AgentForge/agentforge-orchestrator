using System.Text.Json;
using AgentForge.Orchestrator.Hubs;
using AgentForge.Orchestrator.Models.Broker;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgentForge.Orchestrator.Messaging;

public class EventsConsumer : BackgroundService
{
    private readonly IRabbitMqConnection _conn;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IConfiguration _cfg;
    private readonly ILogger<EventsConsumer> _log;
    private IModel? _channel;

    public EventsConsumer(
        IRabbitMqConnection conn,
        IHubContext<ChatHub> hub,
        IConfiguration cfg,
        ILogger<EventsConsumer> log)
    {
        _conn = conn;
        _hub  = hub;
        _cfg  = cfg;
        _log  = log;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _conn.CreateChannel();
        var queue = _cfg["RabbitMQ:EventsQueue"] ?? "agent-events-queue";
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.BasicQos(0, prefetchCount: 32, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var evt = JsonSerializer.Deserialize<AgentEventOccurredDto>(ea.Body.Span)
                          ?? throw new InvalidOperationException("null payload");

                await _hub.Clients
                    .Group(evt.ConversationId.ToString())
                    .SendAsync("SessionEvent", evt, stoppingToken);

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "EventsConsumer: failed to process message");
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue, autoAck: false, consumer);
        _log.LogInformation("EventsConsumer started, listening on {Queue}", queue);

        stoppingToken.Register(() => _channel?.Close());
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}
