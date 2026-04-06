using Apex.Application.DTOs;
using Apex.Domain.Entities;

namespace Apex.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResponse<Order>> GetOrdersAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<OrderResponseDto> PlaceOrderAsync(string customerName, List<OrderItemRequest> items);
}