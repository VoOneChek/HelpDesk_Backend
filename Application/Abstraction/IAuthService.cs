using Application.Common.Result;
using Application.DTOs.User;

namespace Application.Abstraction
{
    public interface IAuthService
    {
        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Аутентификация пользователя по логину и паролю
        /// </summary>
        Task<Result<UserResponseDto>> AuthenticateAsync(LoginDto loginDto);

        /// <summary>
        /// Генерация JWT токена для пользователя
        /// </summary>
        Task<Result<AuthResponseDto>> GenerateToken(Guid userID);

        /// <summary>
        /// Восстановление пароля
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        Task<Result<UserResponseDto>> RecoverLoginAsync(UpdateProfileDto loginDto);

        Task<Result<UserResponseDto>> RecoverPasswordAsync(Guid userID, RecoverPassword loginDto);
    }
}
