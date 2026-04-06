using Apex.Domain.Entities;
using Apex.Domain.Interfaces;
using Apex.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Apex.Domain.Exceptions;

namespace Apex.Infrastructure.Persistence.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    private IGenericRepository<OrderItem>? _orderItems;
    private IGenericRepository<SalesStatistic>? _salesStats;

    // Lazy-loading repositories: They are only created if a Service calls them.
    public IProductRepository Products => field ??= new ProductRepository(_context);

    public IOrderRepository Orders => field ??= new OrderRepository(_context);

    public IUserRepository Users => field ??= new UserRepository(_context);

    public IGenericRepository<OrderItem> OrderItems => _orderItems ??= new GenericRepository<OrderItem>(_context);

    public IGenericRepository<SalesStatistic> SalesStatistics => _salesStats ??= new GenericRepository<SalesStatistic>(_context);

    public async Task<int> CompleteAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Catch the "Database" error and throw a "Domain" error
            throw new ConcurrencyException("The data was modified by another user. Please reload.");
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this); // Best practice for IDisposable
    }
}