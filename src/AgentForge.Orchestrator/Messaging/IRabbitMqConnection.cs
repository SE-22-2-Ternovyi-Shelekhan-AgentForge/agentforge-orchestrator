using RabbitMQ.Client;

namespace AgentForge.Orchestrator.Messaging;

public interface IRabbitMqConnection
{
    IModel CreateChannel();
}
