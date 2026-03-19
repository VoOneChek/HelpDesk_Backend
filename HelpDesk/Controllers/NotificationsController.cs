using Application.Abstraction;
using HelpDesk.Controllers.Common;
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
        private readonly GetCurrentUser _userHelper = new GetCurrentUser();

        public NotificationsController(INotificationService service, GetCurrentUser userHelper)
        {
            _service = service;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            var userId = _userHelper.GetCurrentUserId(User);
            var result = await _service.GetUserNotificationsAsync(userId);
            return Ok(result.Data);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var result = await _service.MarkAsReadAsync(id);
            if (!result.Success) return BadRequest(new { error = result.Error });
            return Ok();
        }
    }
}
