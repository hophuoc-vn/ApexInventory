using Apex.Domain.Common;
using Apex.Domain.Enums;

namespace Apex.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; }
    public string CustomerName { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    // Navigation property: One Order has many Items
    public virtual ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    public Order(string orderNumber, string customerName)
    {
        OrderNumber = orderNumber;
        CustomerName = customerName;
        Status = OrderStatus.Pending;
        TotalAmount = 0;
    }

    private Order() { } // For EF Core

    public void AddItem(Product product, int quantity)
    {
        var item = new OrderItem(product, quantity);
        OrderItems.Add(item);

        TotalAmount += (product.Price * quantity);
    }
}