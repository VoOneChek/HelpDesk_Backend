using Domain.Entities;
using Domain.Enums;

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
        Task<IEnumerable<Ticket>> GetAllWithDetailsAsync(TicketStatus? status, Guid? categoryId, DateTime? from, DateTime? to, string? search);

        /// <summary>
        /// Получение одного тикета с деталями
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ticket?> GetWithDetailsByIdAsync(Guid id);

        /// <summary>
        /// Для статистики
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="status"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        Task<int> CountByOperatorAsync(Guid operatorId, TicketStatus? status = null, DateTime? from = null, DateTime? to = null);
    }
}
