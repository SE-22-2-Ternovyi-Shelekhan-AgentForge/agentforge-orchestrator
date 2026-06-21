using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Orchestrator.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AgentForgeDbContext _context;

    public UserRepository(AgentForgeDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalized);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Users.AnyAsync(u => u.Email == normalized);
    }

    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
