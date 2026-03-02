using Application.DTOs.User;

namespace Application.Abstraction
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto> CreateAsync(CreateUserDto dto);
        Task BlockUserAsync(Guid userId);
    }
}
