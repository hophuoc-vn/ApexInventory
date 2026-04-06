using Apex.Domain.Entities;

namespace Apex.Application.Interfaces;

public interface IProductService
{
    Task<Product?> GetProductBySkuAsync(string sku);
    Task<Product> CreateProductAsync(string name, string sku, decimal price, int stock);
    Task<bool> AdjustStockAsync(string sku, int amount);
    Task<IEnumerable<Product>> GetLowStockAlertsAsync(int threshold);
}