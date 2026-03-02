using Domain.Enums;

namespace Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        // Foreign keys
        public Guid ClientId { get; set; }
        public User Client { get; set; } = null!;

        public Guid? OperatorId { get; set; }
        public User? Operator { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Navigation
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    }
}
