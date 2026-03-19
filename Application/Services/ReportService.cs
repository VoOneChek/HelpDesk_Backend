using Application.Abstraction;
using Application.Common.Result;
using Application.Common.GenerateReportFile;
using Application.DTOs.Report;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ITicketRepository _repository;
        private readonly IMapper _mapper;
        private readonly GenerateExcelFile generateExcelFile;
        private readonly GenerateCsvFile generateCsvFile;

        public ReportService(ITicketRepository repository, IMapper mapper, GenerateExcelFile generateExcelFile, GenerateCsvFile generateCsvFile)
        {
            _repository = repository;
            _mapper = mapper;
            this.generateExcelFile = generateExcelFile;
            this.generateCsvFile = generateCsvFile;
        }

        public async Task<Result<ReportResultDto>> GetReportAsync(ReportFilterDto filter)
        {
            if (filter.From == default || filter.To == default)
                return Result<ReportResultDto>.Fail("Необходимо указать период");

            var tickets = await _repository.GetAllWithDetailsAsync(
                filter.Status,
                filter.CategoryId,
                filter.From,
                filter.To,
                null,
                filter.OperatorId
            );
            var ticketList = tickets.ToList();

            var summary = CalculateSummary(ticketList);

            var details = _mapper.Map<List<ReportItemDto>>(ticketList);

            return Result<ReportResultDto>.Ok(new ReportResultDto
            {
                Summary = summary,
                Details = details
            });
        }

        public async Task<Result<(byte[] FileBytes, string FileName, string ContentType)>> ExportReportAsync(ReportFilterDto filter, string format)
        {
            // 1. Получаем данные
            var tickets = await _repository.GetAllWithDetailsAsync(
                filter.Status,
                filter.CategoryId,
                filter.From,
                filter.To,
                null,
                filter.OperatorId
            );

            var ticketList = tickets.ToList();

            // 2. Считаем статистику
            var summary = CalculateSummary(ticketList);
            var details = _mapper.Map<List<ReportItemDto>>(ticketList);

            foreach (var detail in details)
            {
                var ticket = ticketList.First(t => t.Id == detail.TicketId);
                if (ticket.ClosedAt.HasValue)
                {
                    var duration = ticket.ClosedAt.Value - ticket.CreatedAt;
                    detail.ResolutionTime = $"{Math.Floor(duration.TotalHours)} ч. {duration.Minutes} мин.";
                }
                else
                {
                    detail.ResolutionTime = "-";
                }
            }

            // 3. Генерация файла
            string fileName;
            string contentType;
            byte[] bytes;
            string dateSuffix = $"_{DateTime.Now:yyyyMMdd_HHmm}";

            if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                fileName = $"Report{dateSuffix}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                bytes = generateExcelFile.GenerateExcelWithStats(summary, details);
            }
            else // CSV
            {
                fileName = $"Report{dateSuffix}.csv";
                contentType = "text/csv";
                bytes = generateCsvFile.GenerateCsv(details);
            }

            return Result<(byte[] FileBytes, string FileName, string ContentType)>.Ok((bytes, fileName, contentType));
        }

        private ReportSummaryDto CalculateSummary(List<Ticket> tickets)
        {
            var closedTickets = tickets.Where(t => t.Status == TicketStatus.Closed && t.ClosedAt.HasValue).ToList();

            string avgTime = "-";
            if (closedTickets.Any())
            {
                var totalSeconds = closedTickets.Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalSeconds);
                var span = TimeSpan.FromSeconds(totalSeconds);
                avgTime = $"{span.Days} дн. {span.Hours} ч. {span.Minutes} мин.";
            }

            return new ReportSummaryDto
            {
                TotalTickets = tickets.Count,
                ClosedTickets = closedTickets.Count,
                OpenTickets = tickets.Count(t => t.Status != TicketStatus.Closed),
                AverageResolutionTime = avgTime,
                TopCategories = tickets
                    .Where(t => t.Category != null)
                    .GroupBy(t => t.Category.Name)
                    .Select(g => new CategoryStatsDto { CategoryName = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(5)
                    .ToList(),
                OperatorPerformance = tickets
                    .Where(t => t.Operator != null && t.Status == TicketStatus.Closed)
                    .GroupBy(t => t.Operator!.FullName)
                    .Select(g => new OperatorStatsDto { OperatorName = g.Key, ClosedCount = g.Count() })
                    .OrderByDescending(g => g.ClosedCount)
                    .ToList()
            };
        }
    }
}
