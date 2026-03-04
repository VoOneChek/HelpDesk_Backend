using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using Microsoft.Extensions.Logging;
using System;


namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(IRepository<User> repository, IMapper mapper, ILogger<UserService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<Result<UserResponseDto>> GetCurrentUserAsync(Guid userId)
        {
            _logger.LogInformation("Получение информации о пользователе с ID {UserId}", userId);

            var user = await _repository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден", userId);
                return Result<UserResponseDto>.Fail("Пользователь не найден");
            }

            _logger.LogInformation("Информация о пользователе с ID {UserId} успешно получена", userId);
            return Result<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);

            user.Id = Guid.NewGuid();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsBlocked = false;

            await _repository.AddAsync(user);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task BlockUserAsync(Guid userId)
        {
            var user = await _repository.GetByIdAsync(userId)
                       ?? throw new Exception("User not found");

            user.IsBlocked = true;

            _repository.Update(user);
        }
    }
}
