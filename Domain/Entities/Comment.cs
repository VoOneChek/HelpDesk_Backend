
namespace Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }

        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!;
    }
}
