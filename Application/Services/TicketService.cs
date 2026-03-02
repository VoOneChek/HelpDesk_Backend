using Application.Abstraction;
using Application.DTOs.Ticket;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;
using Infrastructure.Repositories;

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

        public async Task<TicketResponseDto> CreateTicketAsync(Guid clientId, CreateTicketDto dto)
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
            await _ticketRepository.SaveChangesAsync();

            return _mapper.Map<TicketResponseDto>(ticket);
        }

        public async Task<IEnumerable<TicketResponseDto>> GetAllAsync()
        {
            var tickets = await _ticketRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TicketResponseDto>>(tickets);
        }

        public async Task<IEnumerable<TicketResponseDto>> GetClientTicketsAsync(Guid clientId)
        {
            var tickets = await _ticketRepository.GetByClientIdAsync(clientId);
            return _mapper.Map<IEnumerable<TicketResponseDto>>(tickets);
        }

        public async Task AssignOperatorAsync(Guid ticketId, Guid operatorId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                         ?? throw new Exception("Ticket not found");

            ticket.OperatorId = operatorId;
            ticket.Status = TicketStatus.InProgress;

            _ticketRepository.Update(ticket);
            await _ticketRepository.SaveChangesAsync();
        }

        public async Task ChangeStatusAsync(Guid ticketId, TicketStatus status)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                         ?? throw new Exception("Ticket not found");

            ticket.Status = status;

            if (status == TicketStatus.Closed)
                ticket.ClosedAt = DateTime.UtcNow;

            _ticketRepository.Update(ticket);
            await _ticketRepository.SaveChangesAsync();
        }
    }
}
