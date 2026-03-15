using Application.Common.Result;
using Application.DTOs.Ticket;
using Application.DTOs.TicketHistory;
using Application.DTOs.User;
using Domain.Enums;

namespace Application.Abstraction
{
    public interface ITicketService
    {
        // Для клиента
        Task<Result<TicketResponseDto>> CreateTicketAsync(Guid clientId, CreateTicketDto dto);
        Task<Result<IEnumerable<TicketResponseDto>>> GetClientTicketsAsync(Guid clientId);

        // Для оператора
        Task<Result<IEnumerable<TicketResponseDto>>> GetAllAsync(TicketFilterDto filter);
        Task<Result> AssignOperatorAsync(Guid ticketId, Guid operatorId);
        Task<Result> ChangeStatusAsync(Guid ticketId, TicketStatus status, Guid changedByUserId);
        Task<Result<OperatorStatsDto>> GetOperatorStatsAsync(Guid operatorId);

        Task<Result<IEnumerable<TicketHistoryDto>>> GetHistoryAsync(Guid ticketId, Guid currentUserId, UserRole currentUserRole);
    }
}
