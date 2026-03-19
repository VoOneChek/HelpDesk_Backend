
namespace Application.DTOs.TicketHistory
{
    public class TicketHistoryDto
    {
        public string Action { get; set; } = null!;
        public string ChangedBy { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }
}
