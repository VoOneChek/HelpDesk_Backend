using Application.DTOs.Report;

namespace Application.Abstraction
{
    public interface IReportService
    {
        Task<TicketReportDto> GetTicketReportAsync(DateTime from, DateTime to);
        Task<AnalyticsDto> GetAnalyticsAsync();
    }
}
