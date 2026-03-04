using Application.Common.Result;
using Application.DTOs.User;

namespace Application.Abstraction
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllAsync();

        /// <summary>
        /// Получение информации о текущем пользователе
        /// </summary>
        Task<Result<UserResponseDto>> GetCurrentUserAsync(Guid userId);
        Task<UserResponseDto> CreateAsync(CreateUserDto dto);
        Task BlockUserAsync(Guid userId);
    }
}
