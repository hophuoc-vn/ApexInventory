namespace Apex.Application.DTOs;

public record CreateProductRequest(string Name, string Sku, decimal Price, int Stock);