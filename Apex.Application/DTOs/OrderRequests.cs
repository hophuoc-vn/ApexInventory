namespace Apex.Application.DTOs;

public record PlaceOrderRequest(string CustomerName, List<OrderItemRequest> Items);
public record OrderItemRequest(string Sku, int Quantity);