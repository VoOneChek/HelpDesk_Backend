using Application.Common.Result;
using Application.DTOs.Comment;

namespace Application.Abstraction
{
    public interface ICommentService
    {
        Task<Result<CommentResponseDto>> AddCommentAsync(Guid ticketId, Guid authorId, CreateCommentDto dto);
        Task<Result<IEnumerable<CommentResponseDto>>> GetTicketCommentsAsync(Guid ticketId);
    }
}
