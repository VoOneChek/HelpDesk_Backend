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

        public async Task<IEnumerable<Ticket>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByOperatorIdAsync(Guid operatorId)
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.OperatorId == operatorId)
                .ToListAsync();
        }
    }
}
