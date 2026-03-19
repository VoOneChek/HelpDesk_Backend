using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Report
{
    public class ReportFilterDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid? CategoryId { get; set; }
        public TicketStatus? Status { get; set; }
        public Guid? OperatorId { get; set; }
    }

    public class ReportItemDto
    {
        public Guid TicketId { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string ResolutionTime { get; set; } = null!;

        public string CategoryName { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string? OperatorName { get; set; }
    }

    public class ReportSummaryDto
    {
        public int TotalTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int OpenTickets { get; set; }

        public string AverageResolutionTime { get; set; } = null!;

        public List<CategoryStatsDto> TopCategories { get; set; } = new();

        public List<OperatorStatsDto> OperatorPerformance { get; set; } = new();
    }

    public class CategoryStatsDto
    {
        public string CategoryName { get; set; } = null!;
        public int Count { get; set; }
    }

    public class OperatorStatsDto
    {
        public string OperatorName { get; set; } = null!;
        public int ClosedCount { get; set; }
    }

    public class ReportResultDto
    {
        public ReportSummaryDto Summary { get; set; } = null!;
        public List<ReportItemDto> Details { get; set; } = null!;
    }
}
