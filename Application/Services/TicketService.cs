using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.Ticket;
using Application.DTOs.TicketHistory;
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
        private readonly IUserRepository _userRepository;
        private readonly IRepository<TicketHistory> _historyRepo;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public TicketService(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            IRepository<TicketHistory> historyRepo,
            IRepository<Category> categoryRepository,
            IMapper mapper)
        {
            _historyRepo = historyRepo;
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
                filter.SearchString,
                filter.OperatorId
            );
            return Result<IEnumerable<TicketResponseDto>>.Ok(_mapper.Map<IEnumerable<TicketResponseDto>>(tickets));
        }

        public async Task<Result<IEnumerable<TicketHistoryDto>>> GetHistoryAsync(Guid ticketId, Guid currentUserId, UserRole currentUserRole)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null)
                return Result<IEnumerable<TicketHistoryDto>>.Fail("Обращение не найдено");

            if (currentUserRole == UserRole.Client && ticket.ClientId != currentUserId)
            {
                return Result<IEnumerable<TicketHistoryDto>>.Fail("Доступ запрещен");
            }

            var history = await _ticketRepository.GetHistoryByTicketIdAsync(ticketId);
            var dtos = _mapper.Map<IEnumerable<TicketHistoryDto>>(history);

            return Result<IEnumerable<TicketHistoryDto>>.Ok(dtos);
        }

        public async Task<Result<IEnumerable<TicketResponseDto>>> GetClientTicketsAsync(Guid clientId, TicketFilterDto filter)
        {
            var tickets = await _ticketRepository.GetByClientIdAsync(
                clientId,
                filter.Status,
                filter.CategoryId,
                filter.SearchString
                );
            return Result<IEnumerable<TicketResponseDto>>.Ok(_mapper.Map<IEnumerable<TicketResponseDto>>(tickets));
        }

        public async Task<Result> AssignOperatorAsync(Guid ticketId, Guid operatorId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null)
                return Result.Fail("Обращение не найдено");

            var operatorUser = await _userRepository.GetByIdAsync(operatorId);
            if (operatorUser == null) 
                return Result.Fail("Оператор не найден");

            string actionDesc = ticket.OperatorId.HasValue
                ? $"Оператор изменен на {operatorUser.FullName}"
                : $"Назначен оператор {operatorUser.FullName}";

            ticket.OperatorId = operatorId;
            if (ticket.Status == TicketStatus.New) ticket.Status = TicketStatus.InProgress;

            await AddHistoryEntry(ticket, operatorId, actionDesc);

            await _ticketRepository.Update(ticket);
            return Result.Ok();
        }

        public async Task<Result> ChangeStatusAsync(Guid ticketId, TicketStatus status, Guid changedByUserId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null) 
                return Result.Fail("Обращение не найдено");

            var oldStatus = ticket.Status;
            if (oldStatus == status) return Result.Ok();

            ticket.Status = status;

            if (status == TicketStatus.Closed)
                ticket.ClosedAt = DateTime.UtcNow;

            await AddHistoryEntry(ticket, changedByUserId, $"Статус изменен: {oldStatus} -> {status}");

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

        private async Task AddHistoryEntry(Ticket ticket, Guid userId, string action)
        {
            var historyEntry = new TicketHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                ChangedById = userId,
                Action = action,
                ChangedAt = DateTime.UtcNow
            };
            await _historyRepo.AddAsync(historyEntry);
        }

        public async Task<Result<TicketDetailsDto>> GetTicketDetailsAsync(Guid ticketId, Guid currentUserId, UserRole currentUserRole)
        {
            var ticket = await _ticketRepository.GetTicketWithDetailsAsync(ticketId);

            if (ticket == null)
            {
                return Result<TicketDetailsDto>.Fail("Обращение не найдено");
            }

            if (currentUserRole == UserRole.Client && ticket.ClientId != currentUserId)
            {
                return Result<TicketDetailsDto>.Fail("Доступ запрещен");
            }

            var dto = _mapper.Map<TicketDetailsDto>(ticket);

            return Result<TicketDetailsDto>.Ok(dto);
        }
    }
}
