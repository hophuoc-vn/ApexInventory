using Apex.Application.Interfaces;
using Apex.Domain.Entities;
using Apex.Domain.Exceptions;
using Apex.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Apex.Application.DTOs;

namespace Apex.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResponse<Order>> GetOrdersAsync(int pageNumber, int pageSize, string? searchTerm = null)
    {
        // We pass the "Search" logic as a lambda expression
        var (data, totalRecords) = await _unitOfWork.Orders.GetPagedAsync(
            filter: o => string.IsNullOrEmpty(searchTerm) ||
                         o.OrderNumber.Contains(searchTerm) ||
                         o.CustomerName.Contains(searchTerm),
            pageNumber: pageNumber,
            pageSize: pageSize
        );

        return new PagedResponse<Order>(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<OrderResponseDto> PlaceOrderAsync(string customerName, List<OrderItemRequest> items)
    {
        _logger.LogInformation("Processing order request for {CustomerName}", customerName);

        // 1. Initialize the Order Entity
        var orderNumber = $"ORD-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var order = new Order(orderNumber, customerName);

        try
        {
            foreach (var itemRequest in items)
            {
                // 2. Find the product
                var product = await _unitOfWork.Products.GetBySkuAsync(itemRequest.Sku)
                    ?? throw new KeyNotFoundException($"Product {itemRequest.Sku} not found.");

                // 3. ATOMIC UPGRADE: Use the Repository's Shielded Method
                // This ensures RowVersion is checked and cache is invalidated!
                await _unitOfWork.Products.UpdateStockAsync(product.Id, -itemRequest.Quantity);

                // 4. PRICE FREEZE: Lock in the price into the OrderItem
                order.AddItem(product, itemRequest.Quantity);
            }

            // 5. Finalize the Order Record
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            var response = _mapper.Map<OrderResponseDto>(order);
            _logger.LogInformation("Order successfully processed: {OrderNumber}", order.OrderNumber);

            return response;
        }
        catch (Exception ex) when (ex.Message.Contains("Concurrency") || ex is InvalidOperationException)
        {
            // If stock fails or someone else bought it, the transaction rolls back
            _logger.LogWarning("Order failed for {CustomerName}: {Error}", customerName, ex.Message);
            throw; // Re-throw to be caught by the Controller's try-catch
        }
    }
}