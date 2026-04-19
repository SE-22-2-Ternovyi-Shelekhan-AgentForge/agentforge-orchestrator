using AgentForge.Orchestrator.DatabaseContext;
using AgentForge.Orchestrator.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentForge.Orchestrator.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AgentForgeDbContext _context;

        public ConversationRepository(AgentForgeDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation?> RetrieveAsync(Guid id)
        {
            return await _context.Conversations
                .Include(c => c.Team)
                .ThenInclude(t => t.Agents)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ConversationId == id);
        }

        public async Task<IEnumerable<Conversation>> RetrieveByUserIdAsync(Guid userId)
        {
            return await _context.Conversations
                .Include(c => c.Team)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Conversation conversation)
        {
            if (conversation == null)
            {
                throw new ArgumentNullException(nameof(conversation));
            }

            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Conversation conversation)
        {
            if (conversation == null)
            {
                throw new ArgumentNullException(nameof(conversation));
            }

            conversation.UpdatedAt = DateTime.UtcNow;
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var conversation = await _context.Conversations.FindAsync(id);
            if (conversation != null)
            {
                _context.Conversations.Remove(conversation);
                await _context.SaveChangesAsync();
            }
        }
    }
}
