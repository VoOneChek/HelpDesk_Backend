using Application.Abstraction;
using Application.Common.Result;
using Application.Common.Authentication;
using Application.DTOs.User;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;

        public AuthService(IUserRepository repository, IMapper mapper, JwtTokenGenerator tokenGenerator, ILogger<AuthService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Регистрация пользователя с логином {Email}", dto.Email);

            var existingUser = await _repository.GetByLoginAsync(dto.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Попытка регистрации с уже существующим логином {Email}", dto.Email);
                return Result<AuthResponseDto>.Fail("Пользователь с таким логином уже существует");
            }

            var user = _mapper.Map<User>(dto);

            await _repository.AddAsync(user);

            var token = _tokenGenerator.GenerateToken(user);

            _logger.LogInformation("Пользователь {Email} успешно зарегистрирован", dto.Email);

            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Token = token,
                User = _mapper.Map<UserResponseDto>(user)
            });
        }

        public async Task<Result<UserResponseDto>> AuthenticateAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Аутентификация пользователя с логином {Login}", loginDto.Email);

            var user = await _repository.GetByLoginAsync(loginDto.Email);

            if (user == null)
            {
                _logger.LogWarning("Пользователь с логином {Login} не найден", loginDto.Email);
                return Result<UserResponseDto>.Fail("Пользователь не найден");
            }
            else if (user.IsBlocked)
            {
                _logger.LogWarning("Пользователь заблокирован");
                return Result<UserResponseDto>.Fail("Пользователь заблокирован");
            }
            else if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Неверный пароль для пользователя с логином {Login}", loginDto.Email);
                return Result<UserResponseDto>.Fail("Неверный пароль");
            }

            _logger.LogInformation("Пользователь с логином {Login} успешно аутентифицирован", loginDto.Email);
            return Result<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        public async Task<Result<UserResponseDto>> RecoverLoginAsync(UpdateProfileDto loginDto)
        {
            _logger.LogInformation("Восстановление пользователем пароля с логином {Login}", loginDto.FullName);

            var user = await _repository.GetByLoginAsync(loginDto.FullName);

            if (user == null)
            {
                _logger.LogWarning("Пользователь с логином {Login} не найден", loginDto.FullName);
                return Result<UserResponseDto>.Fail("Пользователь не найден");
            }
            else if (user.IsBlocked)
            {
                _logger.LogWarning("Пользователь заблокирован");
                return Result<UserResponseDto>.Fail("Пользователь заблокирован");
            }

            _logger.LogInformation("Пользователь с логином {Login} успешно аутентифицирован", loginDto.FullName);
            return Result<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        public async Task<Result<UserResponseDto>> RecoverPasswordAsync(Guid userID, RecoverPassword loginDto)
        {
            _logger.LogInformation("Восстановление пользователем пароля с ID = {ID}", userID);

            var user = await _repository.GetByIdAsync(userID);

            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID = {ID} не найден", userID);
                return Result<UserResponseDto>.Fail("Пользователь не найден");
            }
            else if (user.IsBlocked)
            {
                _logger.LogWarning("Пользователь заблокирован");
                return Result<UserResponseDto>.Fail("Пользователь заблокирован");
            }
            else if (BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Старый пароль пользователя с ID = {ID} совпадает с новым паролем", userID);
                return Result<UserResponseDto>.Fail("Старый пароль совпадает с новым паролем");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
            await _repository.Update(user);

            _logger.LogInformation("Пользователь с ID = {ID} успешно аутентифицирован", userID);
            return Result<UserResponseDto>.Ok(_mapper.Map<UserResponseDto>(user));
        }

        public async Task<Result<AuthResponseDto>> GenerateToken(Guid userID)
        {
            _logger.LogInformation("Генерация токена для пользователя с ID {UserId}", userID);

            var user = await _repository.GetByIdAsync(userID);

            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден для генерации токена", userID);
                return Result<AuthResponseDto>.Fail("Пользователь не найден");
            }

            var token = _tokenGenerator.GenerateToken(user);

            _logger.LogInformation("Токен успешно сгенерирован для пользователя с ID {UserId}", userID);

            return Result<AuthResponseDto>.Ok(new AuthResponseDto
            {
                Token = token,
                User = _mapper.Map<UserResponseDto>(user)
            });
        }
    }
}
