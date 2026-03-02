
namespace Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
