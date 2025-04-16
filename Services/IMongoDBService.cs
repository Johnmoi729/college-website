using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CollegeWebsite.Services
{
    public interface IMongoDBService<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<List<T>> FindAsync(Expression<Func<T, bool>> filterExpression);
        Task<T> CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task RemoveAsync(string id);
    }
}