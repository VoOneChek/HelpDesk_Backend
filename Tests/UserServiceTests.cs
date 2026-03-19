using Application.DTOs.User;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using Microsoft.Extensions.Logging;
using Moq;

namespace HelpDesk.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UserService>>();

            _service = new UserService(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedUsers()
        {
            // Arrange
            var users = new List<User> { new User { Id = Guid.NewGuid() } };
            var userDtos = new List<UserResponseDto> { new UserResponseDto() };

            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            _mockMapper.Setup(m => m.Map<IEnumerable<UserResponseDto>>(users)).Returns(userDtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetCurrentUserAsync_UserNotFound_ReturnsFail()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.GetCurrentUserAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Пользователь не найден", result.Error);
        }

        [Fact]
        public async Task UpdateProfileAsync_UpdatesNameAndReturnsOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateProfileDto { FullName = "New Name" };
            var user = new User { Id = id, FullName = "Old Name" };

            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

            // Act
            var result = await _service.UpdateProfileAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New Name", user.FullName);
            _mockRepository.Verify(r => r.Update(user), Times.Once);
        }

        [Fact]
        public async Task SwitchBlockUserAsync_TogglesStatusCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var user = new User { Id = id, IsBlocked = false };

            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

            // Act
            var result = await _service.SwitchBlockUserAsync(id);

            // Assert
            Assert.True(result.Success);
            Assert.True(user.IsBlocked);
            _mockRepository.Verify(r => r.Update(user), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_EmailExists_ReturnsFail()
        {
            // Arrange
            var dto = new AdminCreateUserDto { Email = "test@test.com", Password = "123", Role = "Client" };
            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email)).ReturnsAsync(new User());

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Пользователь с таким email уже существует", result.Error);
        }

        [Fact]
        public async Task CreateUserAsync_EmailUnique_CreatesUser()
        {
            // Arrange
            var dto = new AdminCreateUserDto { Email = "test@test.com", Password = "123", Role = "Client", FullName = "Test" };
            var userEntity = new User { Id = Guid.NewGuid() };
            var userDto = new UserResponseDto { Id = userEntity.Id };

            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email)).ReturnsAsync((User?)null);
            _mockMapper.Setup(m => m.Map<User>(dto)).Returns(userEntity);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(userEntity)).Returns(userDto);

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            Assert.True(result.Success);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_EmailTaken_ReturnsFail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new AdminUpdateUserDto { Email = "new@test.com", FullName = "Name", Role = "Client" };
            var existingUser = new User { Id = userId, Email = "old@test.com" };

            var otherUser = new User { Id = Guid.NewGuid() };

            _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockRepository.Setup(r => r.GetByLoginAsync(dto.Email)).ReturnsAsync(otherUser);

            // Act
            var result = await _service.UpdateUserAsync(userId, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("занят", result.Error);
        }
    }
}