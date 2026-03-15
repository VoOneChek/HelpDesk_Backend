using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(HelpDeskDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ticket>> GetAllWithDetailsAsync(TicketStatus? status, Guid? categoryId, DateTime? from, DateTime? to, string? search)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Include(t => t.Operator)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            if (from.HasValue)
                query = query.Where(t => t.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.CreatedAt <= to.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
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

        public async Task<int> CountByOperatorAsync(Guid operatorId, TicketStatus? status, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Tickets.Where(t => t.OperatorId == operatorId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (status == TicketStatus.Closed && from.HasValue)
            {
                query = query.Where(t => t.ClosedAt >= from.Value && (!to.HasValue || t.ClosedAt < to.Value));
            }
            else if (from.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= from.Value && (!to.HasValue || t.CreatedAt < to.Value));
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<TicketHistory>> GetHistoryByTicketIdAsync(Guid ticketId)
        {
            return await _context.TicketHistories
                .Where(h => h.TicketId == ticketId)
                .Include(h => h.ChangedBy)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync();
        }
    }
}
