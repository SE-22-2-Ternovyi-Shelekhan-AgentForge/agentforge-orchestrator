using AgentForge.Orchestrator;
using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Repositories;
using AgentForge.Orchestrator.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AgentForgeDbContext>((DbContextOptionsBuilder options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IAgentTeamRepository, AgentTeamRepository>();
builder.Services.AddScoped<IAgentRepository, AgentRepository>();

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAgentService, AgentService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<OrchestratorProfile>();
});

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
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AgentForgeDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Exception while migrating DB.");
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

app.Run();