using Application.Abstraction;
using Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _userService.GetAllAsync());

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto dto)
            => Ok(await _userService.CreateAsync(dto));

        // PUT: api/users/{id}/block
        [HttpPut("{id}/block")]
        public async Task<IActionResult> Block(Guid id)
        {
            await _userService.BlockUserAsync(id);
            return NoContent();
        }
    }
}
