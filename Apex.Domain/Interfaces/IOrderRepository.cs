using Apex.Domain.Entities;
using Apex.Domain.Models;

namespace Apex.Domain.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<(decimal TotalRevenue, int OrderCount)> GetSalesSummaryAsync(DateTime? since = null);

    Task<OrderDetailsDto?> GetOrderDetailsAsync(Guid id);
}