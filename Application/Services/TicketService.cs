using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.Ticket;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public TicketService(
            ITicketRepository ticketRepository,
            IRepository<User> userRepository,
            IRepository<Category> categoryRepository,
            IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<TicketResponseDto>> CreateTicketAsync(Guid clientId, CreateTicketDto dto)
        {
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Status = TicketStatus.New,
                CreatedAt = DateTime.UtcNow,
                ClientId = clientId,
                CategoryId = dto.CategoryId
            };

            await _ticketRepository.AddAsync(ticket);

            var createdTicket = await _ticketRepository.GetWithDetailsByIdAsync(ticket.Id);
            return Result<TicketResponseDto>.Ok(_mapper.Map<TicketResponseDto>(createdTicket));
        }

        public async Task<Result<IEnumerable<TicketResponseDto>>> GetAllAsync(TicketFilterDto filter)
        {
            var tickets = await _ticketRepository.GetAllWithDetailsAsync(
                filter.Status,
                filter.CategoryId,
                filter.From,
                filter.To,
                filter.SearchString
            );
            return Result<IEnumerable<TicketResponseDto>>.Ok(_mapper.Map<IEnumerable<TicketResponseDto>>(tickets));
        }

        public async Task<Result<IEnumerable<TicketResponseDto>>> GetClientTicketsAsync(Guid clientId)
        {
            var tickets = await _ticketRepository.GetByClientIdAsync(clientId);
            return Result<IEnumerable<TicketResponseDto>>.Ok(_mapper.Map<IEnumerable<TicketResponseDto>>(tickets));
        }

        public async Task<Result> AssignOperatorAsync(Guid ticketId, Guid operatorId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null)
                return Result.Fail("Обращение не найдено");

            ticket.OperatorId = operatorId;
            ticket.Status = TicketStatus.InProgress;

            await _ticketRepository.Update(ticket);
            return Result.Ok();
        }

        public async Task<Result> ChangeStatusAsync(Guid ticketId, TicketStatus status)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null) 
                return Result.Fail("Обращение не найдено");

            ticket.Status = status;

            if (status == TicketStatus.Closed)
                ticket.ClosedAt = DateTime.UtcNow;

            await _ticketRepository.Update(ticket);
            return Result.Ok();
        }

        public async Task<Result<OperatorStatsDto>> GetOperatorStatsAsync(Guid operatorId)
        {
            var inProgressCount = await _ticketRepository.CountByOperatorAsync(operatorId, TicketStatus.InProgress);
            var newAssignedCount = await _ticketRepository.CountByOperatorAsync(operatorId, TicketStatus.New);

            var closedTotal = await _ticketRepository.CountByOperatorAsync(operatorId, TicketStatus.Closed);

            var today = DateTime.UtcNow.Date;
            var closedToday = await _ticketRepository.CountByOperatorAsync(operatorId, TicketStatus.Closed, today, today.AddDays(1));

            return Result<OperatorStatsDto>.Ok(new OperatorStatsDto
            {
                TotalAssigned = inProgressCount + newAssignedCount,
                ClosedTotal = closedTotal,
                ClosedToday = closedToday
            });
        }
    }
}
