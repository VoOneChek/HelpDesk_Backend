using Application.Abstraction;
using Application.DTOs.Ticket;
using Application.Services;
using Domain.Enums;
using HelpDesk.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _service;
        private readonly GetCurrentUser _currentUser;
        private readonly GetCurrentUser _userHelper = new GetCurrentUser();

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
        public async Task<IActionResult> GetMyTickets([FromQuery] TicketFilterDto filter)
        {
            var userId = _currentUser.GetCurrentUserId(User);
            var result = await _service.GetClientTicketsAsync(userId, filter);

            return Ok(result.Data);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Operator, Admin")] 
        public async Task<IActionResult> GetAll([FromQuery] TicketFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketDetails(Guid id)
        {
            var userId = _userHelper.GetCurrentUserId(User);
            var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
            Enum.TryParse<UserRole>(roleStr, out var userRole);

            var result = await _service.GetTicketDetailsAsync(id, userId, userRole);

            if (!result.Success)
                return NotFound(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> AssignOperator(Guid id)
        {
            var operatorId = _userHelper.GetCurrentUserId(User);
            var result = await _service.AssignOperatorAsync(id, operatorId);
            if (!result.Success) return BadRequest(new { error = result.Error });
            return Ok();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Operator, Admin")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] TicketStatus status)
        {
            var operatorId = _userHelper.GetCurrentUserId(User);
            var result = await _service.ChangeStatusAsync(id, status, operatorId);
            if (!result.Success) return BadRequest(new { error = result.Error });
            return Ok();
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Operator")]
        public async Task<IActionResult> GetStats()
        {
            var operatorId = _userHelper.GetCurrentUserId(User);
            var result = await _service.GetOperatorStatsAsync(operatorId);
            return Ok(result.Data);
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            var userId = _userHelper.GetCurrentUserId(User);
            var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
            Enum.TryParse<UserRole>(roleStr, out var userRole);

            var result = await _service.GetHistoryAsync(id, userId, userRole);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }
    }
}
