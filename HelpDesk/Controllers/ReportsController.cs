using Application.Abstraction;
using Application.DTOs.Report;
using Application.Services;
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

        [HttpPost("search")]
        public async Task<IActionResult> GetReport([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetReportAsync(filter);
            if (!result.Success) return BadRequest(new { error = result.Error });
            return Ok(result.Data);
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportReport([FromBody] ReportFilterDto filter, [FromQuery] string format = "excel")
        {

            var result = await _service.ExportReportAsync(filter, format);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            var (fileBytes, fileName, contentType) = result.Data;

            return File(fileBytes, contentType, fileName);
        }
    }
}
