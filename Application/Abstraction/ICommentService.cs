using Application.DTOs.Comment;

namespace Application.Abstraction
{
    public interface ICommentService
    {
        Task<CommentResponseDto> AddCommentAsync(Guid ticketId, Guid authorId, CreateCommentDto dto);
        Task<IEnumerable<CommentResponseDto>> GetTicketCommentsAsync(Guid ticketId);
    }
}
