using Apex.Domain.Common;

namespace Apex.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }

    public virtual Product Product { get; private set; } = null!;
    public virtual Order Order { get; private set; } = null!;

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    public OrderItem(Product product, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");

        ProductId = product.Id;
        Product = product;
        Quantity = quantity;

        UnitPrice = product.Price;
    }

    private OrderItem() { }
}