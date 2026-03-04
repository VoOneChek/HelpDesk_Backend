using Application.Abstraction;
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
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public CommentService(
            IRepository<Comment> repository,
            IRepository<Ticket> ticketRepository,
            IRepository<User> userRepository,
            IMapper mapper)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CommentResponseDto> AddCommentAsync(Guid ticketId, Guid authorId, CreateCommentDto dto)
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

            return _mapper.Map<CommentResponseDto>(comment);
        }

        public async Task<IEnumerable<CommentResponseDto>> GetTicketCommentsAsync(Guid ticketId)
        {
            var comments = (await _repository.GetAllAsync())
                .Where(c => c.TicketId == ticketId);

            return _mapper.Map<IEnumerable<CommentResponseDto>>(comments);
        }
    }
}
