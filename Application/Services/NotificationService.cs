using Application.Abstraction;
using Application.Common.Result;
using Application.DTOs.Notification;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Abstraction;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _repository;
        private readonly IMapper _mapper;

        public NotificationService(IRepository<Notification> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task NotifyAsync(Guid userId, string message)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(notification);
        }

        public async Task<Result<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = (await _repository.GetAllAsync())
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            return Result<IEnumerable<NotificationDto>>.Ok(_mapper.Map<IEnumerable<NotificationDto>>(notifications));
        }

        public async Task<Result> MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId)
                              ?? throw new Exception("Notification not found");

            notification.IsRead = true;

            await _repository.Update(notification);
            return Result.Ok();
        }
    }
}
