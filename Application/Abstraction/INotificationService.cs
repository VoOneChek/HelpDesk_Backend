using Application.Common.Result;
using Application.DTOs.Notification;

namespace Application.Abstraction
{
    public interface INotificationService
    {
        /// <summary>
        /// Создать уведомление
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task NotifyAsync(Guid userId, string message);

        /// <summary>
        /// Получить мои уведомления
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Result<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId);

        /// <summary>
        /// Пометить как прочитанное
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        Task<Result> MarkAsReadAsync(Guid notificationId);
    }
}
