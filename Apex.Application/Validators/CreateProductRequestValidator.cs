using Apex.Application.DTOs;
using Apex.Domain.Interfaces;
using FluentValidation;

namespace Apex.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductRequestValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MinimumLength(5)
            .MustAsync(BeUniqueSku).WithMessage("This SKU is already assigned to another product.");

        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }

    // Custom function to check the Database
    private async Task<bool> BeUniqueSku(string sku, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetBySkuAsync(sku);
        return product == null; // If null, the SKU is unique (Good!)
    }
}