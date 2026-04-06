using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Apex.Domain.Entities;
using Apex.Domain.Interfaces;
using System.Linq.Expressions;

namespace Apex.Application.Services;

public class CachedProductService(IProductRepository repository, IDistributedCache cache) : IProductRepository
{
    private readonly IProductRepository _repository = repository;
    private readonly IDistributedCache _cache = cache;

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        string key = $"product-{sku}";

        var cachedProduct = await _cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cachedProduct))
        {
            return JsonSerializer.Deserialize<Product>(cachedProduct);
        }

        var product = await _repository.GetBySkuAsync(sku);

        if (product != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(product), options);
        }

        return product;
    }

    public Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
        => _repository.GetLowStockProductsAsync(threshold);

    public Task<(IEnumerable<Product> data, int totalCount)> GetPagedAsync(
    Expression<Func<Product, bool>>? filter = null,
    int pageNumber = 1,
    int pageSize = 10)
    {
        return _repository.GetPagedAsync(filter, pageNumber, pageSize);
    }

    public async Task<bool> UpdateStockAsync(Guid productId, int quantityChange)
    {
        // 1. Perform the actual update in the database
        var result = await _repository.UpdateStockAsync(productId, quantityChange);

        if (result)
        {
            // 2. CRITICAL: Find the product to get its SKU iot clear the cache
            var product = await _repository.GetByIdAsync(productId);
            if (product != null)
            {
                await _cache.RemoveAsync($"product-{product.Sku}");
            }
        }

        return result;
    }

    public Task<Product?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);
    public Task<IEnumerable<Product>> GetAllAsync() => _repository.GetAllAsync();
    public Task AddAsync(Product product) => _repository.AddAsync(product);
    public void Update(Product product) => _repository.Update(product);
    public void Delete(Product product) => _repository.Delete(product);
    public void Attach(Product product) => _repository.Attach(product);
}