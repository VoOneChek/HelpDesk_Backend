using Domain.Entities;

namespace Infrastructure.Abstraction
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        /// <summary>
        /// Получение тикетов клиента с подгрузкой связанных данных
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<IEnumerable<Ticket>> GetByClientIdAsync(Guid clientId);

        /// <summary>
        /// Получение всех тикетов (для оператора) с подгрузкой данных
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Ticket>> GetAllWithDetailsAsync();

        /// <summary>
        /// Получение одного тикета с деталями
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ticket?> GetWithDetailsByIdAsync(Guid id);
    }
}
