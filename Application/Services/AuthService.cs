using Application.Abstraction;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;

        public AuthService(IRepository<User> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Client,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = "stub-token",
                User = _mapper.Map<UserResponseDto>(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var users = await _repository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            return new AuthResponseDto
            {
                Token = "stub-token",
                User = _mapper.Map<UserResponseDto>(user)
            };
        }
    }
}
