using Application.Common.Result;
using Application.DTOs.User;

namespace Application.Abstraction
{
    public interface IUserService
    {
        Task<Result<IEnumerable<UserResponseDto>>> GetAllAsync();

        /// <summary>
        /// Получение информации о текущем пользователе
        /// </summary>
        Task<Result<UserResponseDto>> GetCurrentUserAsync(Guid userId);

        /// <summary>
        /// Обновление данных в профиле
        /// </summary>
        Task<Result<UserResponseDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

        /// <summary>
        /// Изменение статуса блокировки пользователя
        /// </summary>
        Task<Result> SwitchBlockUserAsync(Guid userId);

        /// <summary>
        /// Создание пользователя администратором
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<Result<UserResponseDto>> CreateUserAsync(AdminCreateUserDto dto);

        /// <summary>
        /// Обновление данных администратором
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<Result<UserResponseDto>> UpdateUserAsync(Guid userId, AdminUpdateUserDto dto);
    }
}
