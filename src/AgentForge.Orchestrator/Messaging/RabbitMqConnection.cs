using RabbitMQ.Client;

namespace AgentForge.Orchestrator.Messaging;

public class RabbitMqConnection : IRabbitMqConnection, IDisposable
{
    private IConnection? _connection;
    private readonly ConnectionFactory _factory;
    private readonly object _lock = new();

    public RabbitMqConnection(IConfiguration cfg)
    {
        _factory = new ConnectionFactory
        {
            HostName = cfg["RabbitMQ:Host"] ?? "rabbitmq",
            Port     = int.Parse(cfg["RabbitMQ:Port"] ?? "5672"),
            UserName = cfg["RabbitMQ:User"] ?? "guest",
            Password = cfg["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true,
        };
    }

    public IModel CreateChannel()
    {
        if (_connection is null || !_connection.IsOpen)
        {
            lock (_lock)
            {
                if (_connection is null || !_connection.IsOpen)
                    _connection = _factory.CreateConnection("agentforge-orchestrator");
            }
        }

        return _connection.CreateModel();
    }

    public void Dispose() => _connection?.Dispose();
}
