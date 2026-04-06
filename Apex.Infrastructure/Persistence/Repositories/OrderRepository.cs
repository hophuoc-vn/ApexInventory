using Apex.Domain.Entities;
using Apex.Domain.Interfaces;
using Apex.Domain.Models;
using Apex.Infrastructure.Persistence;
using Apex.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace Apex.Infrastructure.Persistence.Repositories;

public class OrderRepository(AppDbContext context) : GenericRepository<Order>(context), IOrderRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(decimal TotalRevenue, int OrderCount)> GetSalesSummaryAsync(DateTime? since = null)
    {
        var query = _dbSet.AsNoTracking();

        if (since.HasValue)
        {
            query = query.Where(o => o.CreatedAt > since.Value);
        }

        var summary = await query
        .GroupBy(_ => 1) // Groups everything into one bucket
        .Select(g => new
        {
            Revenue = g.Sum(o => o.TotalAmount),
            Count = g.Count()
        })
        .FirstOrDefaultAsync();

        return (summary?.Revenue ?? 0, summary?.Count ?? 0);
    }

    public async Task<OrderDetailsDto?> GetOrderDetailsAsync(Guid id)
    {
        return await _context.Orders
            .Where(o => o.Id == id && !o.IsDeleted)
            .Select(o => new OrderDetailsDto
            {
                OrderId = o.Id,
                CustomerName = o.CustomerName,
                TotalAmount = o.TotalAmount,
                Items = o.OrderItems.Select(i => new OrderItemDto
                {
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    SubTotal = i.UnitPrice * i.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }
}