using Domain.Entities;
using Infrastructure.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(HelpDeskDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ticket>> GetAllWithDetailsAsync()
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Include(t => t.Operator)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Include(t => t.Operator)
                .Where(t => t.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<Ticket?> GetWithDetailsByIdAsync(Guid id)
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Include(t => t.Operator)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
