using Apex.Domain.Common;
using System.Linq.Expressions;

namespace Apex.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<(IEnumerable<T> data, int totalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? filter = null,
        int pageNumber = 1,
        int pageSize = 10);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    // Tells EF to track an existing entity without marking it as 'New'
    void Attach(T entity);
}