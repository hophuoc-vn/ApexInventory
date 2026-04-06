using Apex.Domain.Common;

namespace Apex.Domain.Entities;

public class Product : BaseEntity
{
    public byte[] RowVersion { get; private set; }
    public string Name { get; private set; }
    public string Sku { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }

    public Product(string name, string sku, decimal price, int stockQuantity)
    {
        Name = name;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
    }

    private Product() { }

    public void UpdateStock(int amount)
    {
        if (StockQuantity + amount < 0)
            throw new InvalidOperationException("Not enough stock available.");

        StockQuantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }
}