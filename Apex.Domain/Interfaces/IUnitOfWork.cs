using Apex.Domain.Entities;

namespace Apex.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IUserRepository Users { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<SalesStatistic> SalesStatistics { get; }
    Task<int> CompleteAsync(); // This triggers the DB Save
}