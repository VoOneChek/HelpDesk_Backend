using Application.Common.Authentication;
using Application.DTOs.User;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using Microsoft.Extensions.Logging;
using Moq;

namespace HelpDesk.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<IJwtTokenGenerator> _mockTokenGenerator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _mockTokenGenerator = new Mock<IJwtTokenGenerator>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _service = new AuthService(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockTokenGenerator.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_UserAlreadyExists_ReturnsFail()
        {
            // Arrange
            var dto = new RegisterDto { Email = "test@test.com", Password = "123", FullName = "Test User" };

            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email))
                           .ReturnsAsync(new User { Email = dto.Email });

            // Act
            var result = await _service.RegisterAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Пользователь с таким логином уже существует", result.Error);
        }

        [Fact]
        public async Task RegisterAsync_NewUser_ReturnsSuccessAndToken()
        {
            // Arrange
            var dto = new RegisterDto { Email = "new@test.com", Password = "123", FullName = "New User" };
            var userEntity = new User { Id = Guid.NewGuid(), Email = dto.Email };
            var userResponse = new UserResponseDto { Id = userEntity.Id, Email = dto.Email };

            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email)).ReturnsAsync((User?)null);
            _mockMapper.Setup(m => m.Map<User>(dto)).Returns(userEntity);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(userEntity)).Returns(userResponse);
            _mockTokenGenerator.Setup(t => t.GenerateToken(userEntity)).Returns("fake_token");

            // Act
            var result = await _service.RegisterAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("fake_token", result.Data.Token);
            Assert.Equal(userEntity.Email, result.Data.User?.Email);

            _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_UserNotFound_ReturnsFail()
        {
            // Arrange
            var dto = new LoginDto { Email = "wrong@test.com", Password = "123" };
            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.AuthenticateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Пользователь не найден", result.Error);
        }

        [Fact]
        public async Task AuthenticateAsync_UserIsBlocked_ReturnsFail()
        {
            // Arrange
            var dto = new LoginDto { Email = "blocked@test.com", Password = "123" };
            var blockedUser = new User { Email = dto.Email, IsBlocked = true };

            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email)).ReturnsAsync(blockedUser);

            // Act
            var result = await _service.AuthenticateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Пользователь заблокирован", result.Error);
        }

        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var password = "correct_password";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            var dto = new LoginDto { Email = "user@test.com", Password = password };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = hash,
                IsBlocked = false,
                FullName = "Test User"
            };
            var userResponse = new UserResponseDto { Id = user.Id, Email = user.Email };

            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email)).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(userResponse);

            // Act
            var result = await _service.AuthenticateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }
    }
}