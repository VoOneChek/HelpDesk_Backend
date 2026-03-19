using Application.Abstraction;
using Application.Common.Authentication;
using Application.Common.EmailSender;
using Application.DTOs.User;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly TempLoginSessionService _sessionService;
        private readonly EmailService _emailService;

        public AuthController(IAuthService authService, TempLoginSessionService sessionService, EmailService emailService)
        {
            _authService = authService;
            _sessionService = sessionService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.AuthenticateAsync(dto);

            if (!result.Success)
                return Unauthorized(new { error = result.Error });

            var user = result.Data!;

            var code = new Random().Next(100000, 999999).ToString();
            var sessionId = _sessionService.CreateSession(user.Id, code, TimeSpan.FromMinutes(5));

            // выводим в консоль
            Console.WriteLine($"[DEBUG] Verification code for user {user.FullName}: {code}");
            
            var email = user.Email;
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "У пользователя не указана почта" });
            //await _emailService.SendEmailAsync(
            //    to: email,
            //    subject: "Код подтверждения",
            //    body: $"Ваш код подтверждения: {code}");

            return Ok(new VerifyCodeDto
            {
                SessionId = sessionId,
                Code = "email"
            });
        }

        [HttpPost("recover-login")]
        public async Task<IActionResult> RecoverLogin(UpdateProfileDto dto)
        {
            var result = await _authService.RecoverLoginAsync(dto);

            if (!result.Success)
                return Unauthorized(new { error = result.Error });

            var user = result.Data!;

            var code = new Random().Next(100000, 999999).ToString();
            var sessionId = _sessionService.CreateSession(user.Id, code, TimeSpan.FromMinutes(5));

            // выводим в консоль
            Console.WriteLine($"[DEBUG] Verification code for user {user.FullName}: {code}");

            var email = user.Email;
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "У пользователя не указана почта" });
            //await _emailService.SendEmailAsync(
            //    to: email,
            //    subject: "Код подтверждения",
            //    body: $"Ваш код подтверждения: {code}");

            return Ok(new VerifyCodeDto
            {
                SessionId = sessionId,
                Code = "email"
            });
        }

        [HttpPost("recover-password")]
        public async Task<IActionResult> RecoverPassword([FromBody] RecoverPassword dto)
        {
            var session = _sessionService.GetSession(dto.SessionId);

            if (session == null)
                return Unauthorized(new { error = "Сессия не найдена" });

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                _sessionService.RemoveSession(dto.SessionId);
                return Unauthorized(new { error = "Код устарел" });
            }

            if (session.Code != dto.Code)
                return Unauthorized(new { error = "Неверный код" });

            var result = await _authService.RecoverPasswordAsync(session.UserId, dto);

            if (!result.Success)
                return Unauthorized(new { error = result.Error });

            _sessionService.RemoveSession(dto.SessionId);

            var user = result.Data!;

            var tokenResult = await _authService.GenerateToken(user.Id);

            if (!tokenResult.Success)
                return Unauthorized(new { error = tokenResult.Error });

            return Ok(tokenResult.Data);
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> Verify([FromBody] VerifyCodeDto dto)
        {
            var session = _sessionService.GetSession(dto.SessionId);

            if (session == null)
                return Unauthorized(new { error = "Сессия не найдена" });

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                _sessionService.RemoveSession(dto.SessionId);
                return Unauthorized(new { error = "Код устарел" });
            }

            if (session.Code != dto.Code)
                return Unauthorized(new { error = "Неверный код" });

            var tokenResult = await _authService.GenerateToken(session.UserId);
            _sessionService.RemoveSession(dto.SessionId);

            if (!tokenResult.Success)
                return Unauthorized(new { error = tokenResult.Error });

            return Ok(tokenResult.Data);
        }

        [HttpPost("cancel-session")]
        public IActionResult CancelSession([FromBody] string sessionId)
        {
            _sessionService.RemoveSession(sessionId);
            return Ok(new { message = "Сессия отменена" });
        }
    }
}
