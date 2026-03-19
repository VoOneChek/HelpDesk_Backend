using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;
using Microsoft.Extensions.Logging;
using System;


namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper, ILogger<UserService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<UserResponseDto>>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return Result<IEnumerable<UserResponseDto>>.Ok(_mapper.Map<IEnumerable<UserResponseDto>>(users));
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

        public async Task<Result<UserResponseDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _repository.GetByIdAsync(userId);

            if (user == null)
            {
                return Result<UserResponseDto>.Fail("Пользователь не найден");
            }

            user.FullName = dto.FullName;

            await _repository.Update(user);

            _logger.LogInformation("Профиль пользователя {UserId} успешно обновлен", userId);
            return Result<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        public async Task<Result> SwitchBlockUserAsync(Guid userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                return Result.Fail("User not found");

            user.IsBlocked = !user.IsBlocked;
            await _repository.Update(user);

            _logger.LogInformation("Пользователь {UserId} был {Status}", userId, user.IsBlocked ? "заблокирован" : "разблокирован");
            return Result.Ok();
        }

        public async Task<Result<UserResponseDto>> CreateUserAsync(AdminCreateUserDto dto)
        {
            var existingUser = await _repository.GetByLoginAsync(dto.Email);
            if (existingUser != null)
                return Result<UserResponseDto>.Fail("Пользователь с таким email уже существует");

            var user = _mapper.Map<User>(dto);

            await _repository.AddAsync(user);
            return Result<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        public async Task<Result<UserResponseDto>> UpdateUserAsync(Guid userId, AdminUpdateUserDto dto)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return Result<UserResponseDto>.Fail("Пользователь не найден");

            if (user.Email != dto.Email)
            {
                var checkEmail = await _repository.GetByLoginAsync(dto.Email);
                if (checkEmail != null) return Result<UserResponseDto>.Fail("Email уже занят");
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = Enum.Parse<UserRole>(dto.Role);

            await _repository.Update(user);
            return Result<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }
    }
}
