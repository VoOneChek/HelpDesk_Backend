using Application.Abstraction;
using Application.DTOs.Faq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/faq")]
    public class FaqController : ControllerBase
    {
        private readonly IFaqService _service;

        public FaqController(IFaqService service)
        {
            _service = service;
        }

        // GET: api/faq
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        // POST: api/faq
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateFaqDto dto)
            => Ok(await _service.CreateAsync(dto));
    }
}
