using Application.Common.Result;
using Application.DTOs.Ticket;
using Application.DTOs.User;
using Domain.Enums;

namespace Application.Abstraction
{
    public interface ITicketService
    {
        Task<Result<TicketResponseDto>> CreateTicketAsync(Guid clientId, CreateTicketDto dto);
        Task<Result<IEnumerable<TicketResponseDto>>> GetClientTicketsAsync(Guid clientId);

        // Методы для Оператора (сделаем заглушки или базовую реализацию)
        Task<Result> AssignOperatorAsync(Guid ticketId, Guid operatorId);
        Task<Result> ChangeStatusAsync(Guid ticketId, TicketStatus status);
        Task<Result<IEnumerable<TicketResponseDto>>> GetAllAsync();
    }
}
