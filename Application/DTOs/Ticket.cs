using Application.DTOs.Comment;
using Domain.Enums;

namespace Application.DTOs.Ticket
{
    public class CreateTicketDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketPriority Priority { get; set; }
        public Guid CategoryId { get; set; }
    }

    public class TicketResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public string ClientName { get; set; } = null!;
        public string? OperatorName { get; set; }

        public string CategoryName { get; set; } = null!;
    }

    public class TicketDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string ClientName { get; set; } = null!;
        public string? OperatorName { get; set; }
        public string CategoryName { get; set; } = null!;

        public ICollection<CommentResponseDto> Comments { get; set; } = new List<CommentResponseDto>();
    }

    public class TicketFilterDto
    {
        public TicketStatus? Status { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? SearchString { get; set; }
        public Guid? OperatorId { get; set; }
    }
}
