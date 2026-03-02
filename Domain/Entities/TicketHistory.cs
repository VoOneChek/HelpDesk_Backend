
namespace Domain.Entities
{
    public class TicketHistory
    {
        public Guid Id { get; set; }

        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public Guid ChangedById { get; set; }
        public User ChangedBy { get; set; } = null!;

        public string Action { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }
}
