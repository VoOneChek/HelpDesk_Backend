using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.User;
using HelpDesk.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly GetCurrentUser _currentUser;

        public UsersController(IUserService userService, GetCurrentUser currentUser)
        {
            _userService = userService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _userService.GetAllAsync());

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = _currentUser.GetCurrentUserId(User);
            var result = await _userService.GetCurrentUserAsync(userId);

            if (!result.Success)
                return NotFound(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = _currentUser.GetCurrentUserId(User);
            var result = await _userService.UpdateProfileAsync(userId, dto);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPut("{id}/switchBlock")]
        public async Task<IActionResult> SwitchBlock(Guid id)
        {
            await _userService.SwitchBlockUserAsync(id);
            return Ok();
        }
    }
}
