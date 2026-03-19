using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstraction
{
    public interface ICommentRepository: IRepository<Comment>
    {
        /// <summary>
        /// Получение комментариев к тикету с именами авторов
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        Task<IEnumerable<Comment>> GetByTicketIdAsync(Guid ticketId);
    }
}
