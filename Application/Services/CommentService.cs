using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.Comment;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _repository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly INotificationService _notificationService;
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public CommentService(
            IRepository<Comment> repository,
            IRepository<Ticket> ticketRepository,
            IRepository<User> userRepository,
            INotificationService notificationService,
            IMapper mapper)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<Result<CommentResponseDto>> AddCommentAsync(Guid ticketId, Guid authorId, CreateCommentDto dto)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                         ?? throw new Exception("Ticket not found");

            var user = await _userRepository.GetByIdAsync(authorId)
                       ?? throw new Exception("User not found");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                TicketId = ticketId,
                AuthorId = authorId
            };

            await _repository.AddAsync(comment);

            Guid? notifyUserId = authorId == ticket.ClientId ? ticket.OperatorId : ticket.ClientId;

            if (notifyUserId.HasValue && user != null)
            {
                await _notificationService.NotifyAsync(notifyUserId.Value, $"Новый комментарий к тикету #{ticketId}: \"{dto.Content.Substring(0, Math.Min(20, dto.Content.Length))}...\"");
            }

            return Result<CommentResponseDto>.Ok(_mapper.Map<CommentResponseDto>(comment));
        }

        public async Task<Result<IEnumerable<CommentResponseDto>>> GetTicketCommentsAsync(Guid ticketId)
        {
            var comments = (await _repository.GetAllAsync())
                .Where(c => c.TicketId == ticketId);

            return Result<IEnumerable<CommentResponseDto>>.Ok(_mapper.Map<IEnumerable<CommentResponseDto>>(comments));
        }
    }
}
