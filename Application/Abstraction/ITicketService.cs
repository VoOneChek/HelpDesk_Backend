using Application.DTOs.Ticket;
using Application.DTOs.User;
using Domain.Enums;

namespace Application.Abstraction
{
    public interface ITicketService
    {
        Task<TicketResponseDto> CreateTicketAsync(Guid clientId, CreateTicketDto dto);
        Task<IEnumerable<TicketResponseDto>> GetClientTicketsAsync(Guid clientId);
        Task AssignOperatorAsync(Guid ticketId, Guid operatorId);
        Task ChangeStatusAsync(Guid ticketId, TicketStatus status);
        Task<IEnumerable<TicketResponseDto>> GetAllAsync();
    }
}
