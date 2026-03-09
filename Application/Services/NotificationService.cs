using Application.Abstraction;
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

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = (await _repository.GetAllAsync())
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId)
                              ?? throw new Exception("Notification not found");

            notification.IsRead = true;

            await _repository.Update(notification);
        }
    }
}
