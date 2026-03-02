using Application.Abstraction;
using Application.DTOs.Ticket;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _service;

        public TicketsController(ITicketService service)
        {
            _service = service;
        }

        // POST: api/tickets
        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            return Ok(await _service.CreateTicketAsync(userId, dto));
        }

        // GET: api/tickets/my (для клиента)
        [HttpGet("my")]
        public async Task<IActionResult> MyTickets()
        {
            var userId = Guid.Parse(User.FindFirst("id")!.Value);
            return Ok(await _service.GetClientTicketsAsync(userId));
        }

        // GET: api/tickets (оператор / админ)
        [Authorize(Roles = "Operator,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        // PUT: api/tickets/{id}/assign/{operatorId}
        [Authorize(Roles = "Operator,Admin")]
        [HttpPut("{id}/assign/{operatorId}")]
        public async Task<IActionResult> Assign(Guid id, Guid operatorId)
        {
            await _service.AssignOperatorAsync(id, operatorId);
            return NoContent();
        }

        // PUT: api/tickets/{id}/status
        [Authorize(Roles = "Operator,Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, TicketStatus status)
        {
            await _service.ChangeStatusAsync(id, status);
            return NoContent();
        }
    }
}
