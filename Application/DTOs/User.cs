
namespace Application.DTOs.User
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsBlocked { get; set; }
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = null!;

        // Если клиенту можно менять почту, добавляем сюда, но потребуется проверка на уникальность
        // public string Email { get; set; } = null!; 
    }

    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }

    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class VerifyCodeDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public UserResponseDto? User { get; set; } = null;
    }
}
