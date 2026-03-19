using Domain.Entities;
using Infrastructure;
using Infrastructure.Abstraction;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CommentRepository: Repository<Comment>, ICommentRepository
    {
        public CommentRepository(HelpDeskDbContext context) : base(context) { }

        public async Task<IEnumerable<Comment>> GetByTicketIdAsync(Guid ticketId)
        {
            return await _context.Comments
                .Where(c =>  c.TicketId == ticketId)
                .ToListAsync();
        }
    }
}