using Apex.Domain.Entities;
using Apex.Domain.Interfaces;
using Apex.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Apex.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext context) : GenericRepository<Product>(context), IProductRepository
{
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _context.Products
            .Where(p => p.StockQuantity <= threshold)
            .ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(Guid productId, int quantityChange)
    {
        var product = await _context.Products.FindAsync(productId);

        if (product == null) return false;

        try
        {
            // This calls the Domain Method in Product.cs
            product.UpdateStock(quantityChange);

            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // This is the "Shield" in action. Someone else updated this product
            // between Read and Write.
            throw new Exception("The product stock was updated by another user. Please refresh and try again.");
        }
    }
}