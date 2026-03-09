
namespace Infrastructure.Abstraction
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task Update(T entity);
        Task Delete(T entity);
    }
}
