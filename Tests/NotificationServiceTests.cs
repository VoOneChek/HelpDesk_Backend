using Application.DTOs.Notification;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;
using Moq;

namespace HelpDesk.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IRepository<Notification>> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _mockRepository = new Mock<IRepository<Notification>>();
            _mockMapper = new Mock<IMapper>();
            _service = new NotificationService(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task NotifyAsync_CreatesNotificationAndSaves()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var message = "Test message";

            // Act
            await _service.NotifyAsync(userId, message);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(It.Is<Notification>(n =>
                n.UserId == userId &&
                n.Message == message &&
                n.IsRead == false)), Times.Once);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_FiltersByUserAndOrdersDescending()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var dateOld = DateTime.UtcNow.AddMinutes(-10);
            var dateNew = DateTime.UtcNow;

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), UserId = targetUserId, CreatedAt = dateOld, Message = "Old" },
                new Notification { Id = Guid.NewGuid(), UserId = otherUserId, CreatedAt = dateNew, Message = "Other User" },
                new Notification { Id = Guid.NewGuid(), UserId = targetUserId, CreatedAt = dateNew, Message = "New" }
            };

            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(notifications);

            _mockMapper.Setup(m => m.Map<IEnumerable<NotificationDto>>(It.IsAny<IEnumerable<Notification>>()))
                .Returns((IEnumerable<Notification> source) =>
                    source.Select(n => new NotificationDto { Message = n.Message, CreatedAt = n.CreatedAt }));

            // Act
            var result = await _service.GetUserNotificationsAsync(targetUserId);

            // Assert
            Assert.True(result.Success);
            var resultList = result.Data?.ToList();

            Assert.Equal(2, resultList?.Count);

            Assert.Equal("New", resultList?[0].Message);
            Assert.Equal("Old", resultList?[1].Message);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationExists_UpdatesToRead()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            var notification = new Notification { Id = notificationId, IsRead = false };

            _mockRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

            // Act
            var result = await _service.MarkAsReadAsync(notificationId);

            // Assert
            Assert.True(result.Success);
            Assert.True(notification.IsRead);
            _mockRepository.Verify(r => r.Update(notification), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationNotFound_ThrowsException()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync((Notification?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.MarkAsReadAsync(notificationId));
        }
    }
}