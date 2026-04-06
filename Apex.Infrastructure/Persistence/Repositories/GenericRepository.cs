using Apex.Domain.Common;
using Apex.Domain.Interfaces;
using Apex.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apex.Infrastructure.Persistence.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id) => await _context.Set<T>().FindAsync(id);

    public IQueryable<T> GetAllReadOnly() => _context.Set<T>().AsNoTracking();

    public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();

    public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

    public void Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        _dbSet.Update(entity);
    }

    public async Task<(IEnumerable<T> data, int totalCount)> GetPagedAsync(
    Expression<Func<T, bool>>? filter = null,
    int pageNumber = 1,
    int pageSize = 10)
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalCount);
    }

    public void Attach(T entity)
    {
        _dbSet.Attach(entity);
    }
}