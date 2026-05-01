using System.Linq.Expressions;

namespace CargoAPI.DataAccess.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);
        Task SaveAsync();
    }
}
