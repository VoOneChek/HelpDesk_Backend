using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Report
{
    public class TicketReportDto
    {
        public int TotalTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int OpenTickets { get; set; }
    }

    public class AnalyticsDto
    {
        public double AverageResponseTimeHours { get; set; }
        public int ClosedTickets { get; set; }
    }
}
