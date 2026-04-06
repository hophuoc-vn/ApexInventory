using FluentValidation;
using Apex.Application.DTOs;

namespace Apex.Application.Validators;

public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        // Rule: Customer name cannot be empty and shouldn't be a 2000-character novel
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100).WithMessage("Name is too long.");

        // Rule: You can't place an order with 0 items
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("An order must have at least one item.");

        // Rule: Check every single item in the list
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Sku).NotEmpty().WithMessage("SKU is required.");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        });
    }
}