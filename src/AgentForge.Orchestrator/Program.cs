using AgentForge.Orchestrator;
using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Hubs;
using AgentForge.Orchestrator.Messaging;
using AgentForge.Orchestrator.Repositories;
using AgentForge.Orchestrator.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AgentForgeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IAgentTeamRepository, AgentTeamRepository>();
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IAgentSessionTraceRepository, AgentSessionTraceRepository>();

// Services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAgentService, AgentService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<OrchestratorProfile>());

// Messaging (RabbitMQ)
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<ISessionPublisher, SessionPublisher>();
builder.Services.AddHostedService<EventsConsumer>();
builder.Services.AddHostedService<ResultsConsumer>();
builder.Services.AddHostedService<ErrorsConsumer>();

// SignalR
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AgentForge Orchestrator API", Version = "v1" });
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("DatabaseSettings:RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    try
    {
        scope.ServiceProvider.GetRequiredService<AgentForgeDbContext>().Database.Migrate();
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogError(ex, "Exception while migrating DB.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgentForge Orchestrator API v1");
    c.RoutePrefix = string.Empty;
});

app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
