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
        Task<IEnumerable<Ticket>> GetByClientIdAsync(Guid clientId, TicketStatus? status, Guid? categoryId, string? search);

        /// <summary>
        /// Получение всех тикетов (для оператора) с подгрузкой данных
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Ticket>> GetAllWithDetailsAsync(TicketStatus? status, Guid? categoryId, DateTime? from, DateTime? to, string? search, Guid? operatorId);

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

        /// <summary>
        /// Получение истории тикета по Id
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        Task<IEnumerable<TicketHistory>> GetHistoryByTicketIdAsync(Guid ticketId);

        /// <summary>
        /// Получение карточки обращения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ticket?> GetTicketWithDetailsAsync(Guid id);
    }
}
