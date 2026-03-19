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

        public async Task<IEnumerable<Ticket>> GetAllWithDetailsAsync(TicketStatus? status, Guid? categoryId, DateTime? from, DateTime? to, string? search, Guid? operatorId)
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
            {
                var utcFrom = from.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc)
                    : from.Value.ToUniversalTime();

                query = query.Where(t => t.CreatedAt >= utcFrom);
            }

            if (to.HasValue)
            { 
                var utcTo = to.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc)
                    : to.Value.ToUniversalTime();

                query = query.Where(t => t.CreatedAt <= utcTo);
            }

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            if (operatorId.HasValue)
                query = query.Where(t => t.OperatorId == operatorId.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByClientIdAsync(Guid clientId, TicketStatus? status, Guid? categoryId, string? search)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Client)
                .Include(t => t.Operator)
                .Where(t => t.ClientId == clientId)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
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
                .Include(h => h.ChangedBy)
                .Where(h => h.TicketId == ticketId)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketWithDetailsAsync(Guid id)
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
