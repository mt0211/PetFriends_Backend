

namespace DataAccess.Repositories;

    public interface IRepository<T> where T : class
    {
        Task<T?> Get(Guid id);
        Task<Guid> Insert(T entity);
        Task<bool> Update(T entity);
        Task<bool> Remove(T entity);
        Task AddRange(List<T> entities);
        Task DeleteRange(List<T> entities);
        Task UpdateRange(List<T> entities);
       // Task<User> GetUserByID(Guid id);
    }



