using AgentForge.Orchestrator.Models;

namespace AgentForge.Orchestrator.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task CreateAsync(User user);
}
