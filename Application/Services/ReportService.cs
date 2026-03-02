using Application.Abstraction;
using Application.DTOs.Report;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IRepository<Ticket> _repository;

        public ReportService(IRepository<Ticket> repository)
        {
            _repository = repository;
        }

        public async Task<TicketReportDto> GetTicketReportAsync(DateTime from, DateTime to)
        {
            var tickets = (await _repository.GetAllAsync())
                .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
                .ToList();

            return new TicketReportDto
            {
                TotalTickets = tickets.Count,
                ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
                OpenTickets = tickets.Count(t => t.Status != TicketStatus.Closed)
            };
        }

        public async Task<AnalyticsDto> GetAnalyticsAsync()
        {
            var tickets = (await _repository.GetAllAsync())
                .Where(t => t.ClosedAt != null)
                .ToList();

            double avg = 0;

            if (tickets.Any())
            {
                avg = tickets.Average(t =>
                    (t.ClosedAt!.Value - t.CreatedAt).TotalHours);
            }

            return new AnalyticsDto
            {
                AverageResponseTimeHours = avg,
                ClosedTickets = tickets.Count
            };
        }
    }
}
