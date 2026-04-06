namespace Apex.Application.DTOs;

public record OrderResponseDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    decimal TotalAmount,
    DateTime OrderDate,
    List<OrderItemResponseDto> Items);

public record OrderItemResponseDto(
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice);