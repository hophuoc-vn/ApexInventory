using Apex.Application.DTOs;
using Apex.Application.Interfaces;
using Apex.Domain.Entities;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku)
    {
        var product = await _productService.GetProductBySkuAsync(sku);

        if (product == null) return NotFound($"Product with SKU {sku} not found.");

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, [FromServices] IValidator<CreateProductRequest> validator)
    {
        // 1. Run the Validation
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        // 2. Proceed if valid
        var product = await _productService.CreateProductAsync(
            request.Name, request.Sku, request.Price, request.Stock);

        return CreatedAtAction(nameof(GetBySku), new { sku = product.Sku }, product);
    }

    [HttpPost("{sku}/adjust-stock")]
    public async Task<IActionResult> AdjustStock(string sku, [FromBody] int amount)
    {
        var success = await _productService.AdjustStockAsync(sku, amount);

        if (!success)
            return NotFound($"Product with SKU {sku} not found.");

        return Ok(new { message = "Stock updated successfully." });
    }
}