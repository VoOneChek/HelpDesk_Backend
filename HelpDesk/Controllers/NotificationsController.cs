using Application.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            return Ok(await _service.GetUserNotificationsAsync(userId));
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            await _service.MarkAsReadAsync(id);
            return NoContent();
        }
    }
}
