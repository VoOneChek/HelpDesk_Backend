using Domain.Entities;

namespace Infrastructure.Abstraction
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetByClientIdAsync(Guid clientId);
        Task<IEnumerable<Ticket>> GetByOperatorIdAsync(Guid operatorId);
    }
}
