using Application.Abstraction;
using Application.DTOs.Comment;
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

        public CommentsController(ICommentService service)
        {
            _service = service;
        }

        // POST: api/tickets/{ticketId}/comments
        [HttpPost]
        public async Task<IActionResult> Add(Guid ticketId, CreateCommentDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            return Ok(await _service.AddCommentAsync(ticketId, userId, dto));
        }

        // GET: api/tickets/{ticketId}/comments
        [HttpGet]
        public async Task<IActionResult> Get(Guid ticketId)
            => Ok(await _service.GetTicketCommentsAsync(ticketId));
    }
}
