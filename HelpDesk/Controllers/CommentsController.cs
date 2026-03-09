using Application.Abstraction;
using Application.DTOs.Comment;
using Application.Services;
using HelpDesk.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/tickets/{ticketId}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly GetCurrentUser _currentUser;

        public CommentsController(ICommentService service, GetCurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(Guid ticketId)
        {
            var result = await _service.GetTicketCommentsAsync(ticketId);
            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid ticketId, [FromBody] CreateCommentDto dto)
        {
            var userId = _currentUser.GetCurrentUserId(User);
            var result = await _service.AddCommentAsync(ticketId, userId, dto);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }
    }
}
