using Apex.Domain.Entities;
using Apex.Domain.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Apex.Api.Controllers;

[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class DashboardController(IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSalesSummary()
    {
        var stats = (await _unitOfWork.SalesStatistics.GetAllAsync()).FirstOrDefault();

        if (stats == null)
        {
            return NotFound("No statistics have been calculated yet. Please wait for the background worker.");
        }

        var (liveRevenue, liveCount) = await _unitOfWork.Orders.GetSalesSummaryAsync();

        return Ok(new
        {
            //FromSummaryTable = new
            //{
                TotalRevenue = stats?.TotalRevenue ?? 0,
                TotalOrders = stats?.TotalOrders ?? 0,
                LastUpdated = stats?.LastUpdated
            //},
            //LiveFromDatabase = new
            //{
            //    TotalRevenue = liveRevenue,
            //    TotalOrders = liveCount
            //},
            //SystemStatus = stats == null ? "Worker has not run yet" : "Healthy"
        });
    }

    [HttpPost("seed-orders/{count}")]
    public async Task<IActionResult> SeedOrders(int count = 100)
    {
        try
        {
            var random = new Random();
            // 1. Fetch products
            var (data, _) = await _unitOfWork.Products.GetPagedAsync(p => true, 1, 50);
            var products = data.ToList();

            if (!products.Any()) return BadRequest("No products found.");

            for (int i = 0; i < count; i++)
            {
                var orderNumber = $"SEED-{Guid.NewGuid().ToString()[..8].ToUpper()}";
                var customerName = $"Test Customer {random.Next(1, 1000)}";

                var order = new Order(orderNumber, customerName);

                int itemsInOrder = random.Next(1, 4);
                for (int j = 0; j < itemsInOrder; j++)
                {
                    var randomProduct = products[random.Next(products.Count)];

                    _unitOfWork.Products.Attach(randomProduct);

                    order.AddItem(randomProduct, random.Next(1, 5));
                }

                await _unitOfWork.Orders.AddAsync(order);
            }

            await _unitOfWork.CompleteAsync();
            return Ok($"{count} orders seeded successfully.");
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? "No inner exception";
            return StatusCode(500, $"Database Error: {ex.Message}. Inner: {inner}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails(Guid id)
    {
        var order = await _unitOfWork.Orders.GetOrderDetailsAsync(id);

        if (order == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found." });
        }

        return Ok(order);
    }
}