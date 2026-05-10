using System.Text.Json;
using AgentForge.Orchestrator.Models.Broker;
using RabbitMQ.Client;

namespace AgentForge.Orchestrator.Messaging;

public interface ISessionPublisher
{
    void PublishSession(AgentSessionRequestedDto session);
}

public class SessionPublisher : ISessionPublisher
{
    private readonly IRabbitMqConnection _conn;
    private readonly string _queue;
    private readonly ILogger<SessionPublisher> _log;

    public SessionPublisher(IRabbitMqConnection conn, IConfiguration cfg, ILogger<SessionPublisher> log)
    {
        _conn  = conn;
        _queue = cfg["RabbitMQ:SessionsQueue"] ?? "agent-sessions-queue";
        _log   = log;
    }

    public void PublishSession(AgentSessionRequestedDto session)
    {
        using var channel = _conn.CreateChannel();
        channel.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var body  = JsonSerializer.SerializeToUtf8Bytes(session);
        var props = channel.CreateBasicProperties();
        props.ContentType  = "application/json";
        props.DeliveryMode = 2;
        props.MessageId    = session.SessionId.ToString();

        channel.BasicPublish(exchange: "", routingKey: _queue, basicProperties: props, body: body);

        _log.LogInformation("Published session {SessionId} (conversation={ConvId}, agents={Count})",
            session.SessionId, session.ConversationId, session.Team.Agents.Count);
    }
}
