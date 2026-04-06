using Apex.Domain.Entities;

namespace Apex.Domain.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    // Special method just for Products
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    Task<bool> UpdateStockAsync(Guid productId, int quantityChange);
}