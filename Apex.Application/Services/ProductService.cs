using Apex.Application.Interfaces;
using Apex.Domain.Interfaces;
using Apex.Domain.Entities;

namespace Apex.Application.Services;

public class ProductService(IUnitOfWork unitOfWork) : IProductService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Product?> GetProductBySkuAsync(string sku)
    {
        return await _unitOfWork.Products.GetBySkuAsync(sku);
    }

    public async Task<Product> CreateProductAsync(string name, string sku, decimal price, int stock)
    {
        // 1. Create the Domain Entity (The entity handles validation)
        var product = new Product(name, sku, price, stock);

        // 2. Tell the Repository to "Track" this new object
        await _unitOfWork.Products.AddAsync(product);

        // 3. Tell the Unit of Work to commit the transaction to SQL
        await _unitOfWork.CompleteAsync();

        return product;
    }

    public async Task<bool> AdjustStockAsync(string sku, int amount)
    {
        // 1. Find the product first to get its ID
        var product = await _unitOfWork.Products.GetBySkuAsync(sku);
        if (product == null) return false;

        // 2. Use the new Atomic Update method we just wrote in the Repository
        return await _unitOfWork.Products.UpdateStockAsync(product.Id, amount);
    }

    public async Task<IEnumerable<Product>> GetLowStockAlertsAsync(int threshold)
    {
        return await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
    }
}