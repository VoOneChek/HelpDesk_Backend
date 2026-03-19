using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/logs")]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        [HttpGet("download")]
        public IActionResult DownloadLogs()
        {
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            if (!Directory.Exists(logDirectory))
                return NotFound("Папка с логами не найдена");

            var latestFile = Directory.GetFiles(logDirectory)
                .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                .FirstOrDefault();

            if (latestFile == null)
                return NotFound("Файлы логов не найдены");

            var fileName = Path.GetFileName(latestFile);

            try
            {
                using (var fileStream = new FileStream(latestFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    return File(memoryStream.ToArray(), "text/plain", fileName);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Не удалось прочитать файл логов: " + ex.Message });
            }
        }
    }
}
