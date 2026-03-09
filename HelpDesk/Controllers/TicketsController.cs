using Application.Abstraction;
using Application.DTOs.Ticket;
using Application.Services;
using Domain.Enums;
using HelpDesk.Controllers.Common;
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
        private readonly GetCurrentUser _currentUser;

        public TicketsController(ITicketService service, GetCurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
        {
            var userId = _currentUser.GetCurrentUserId(User);
            var result = await _service.CreateTicketAsync(userId, dto);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = _currentUser.GetCurrentUserId(User);
            var result = await _service.GetClientTicketsAsync(userId);

            return Ok(result.Data);
        }

        [HttpGet]
        [Authorize(Roles = "Operator, Admin")] 
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result.Data);
        }

        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> AssignOperator(Guid id, [FromBody] Guid operatorId)
        {
            var result = await _service.AssignOperatorAsync(id, operatorId);
            if (!result.Success) return BadRequest(new { error = result.Error });
            return Ok();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] TicketStatus status)
        {
            var result = await _service.ChangeStatusAsync(id, status);
            if (!result.Success) return BadRequest(new { error = result.Error });
            return Ok();
        }
    }
}
