using Application.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service)
        {
            _service = service;
        }

        // GET: api/reports/tickets?from=2025-01-01&to=2025-12-31
        [HttpGet("tickets")]
        public async Task<IActionResult> GetReport(DateTime from, DateTime to)
            => Ok(await _service.GetTicketReportAsync(from, to));

        // GET: api/reports/analytics
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
            => Ok(await _service.GetAnalyticsAsync());
    }
}
