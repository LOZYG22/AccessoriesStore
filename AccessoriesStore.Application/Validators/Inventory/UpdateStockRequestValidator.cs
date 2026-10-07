using AccessoriesStore.Application.DTOs.Inventory;
using FluentValidation;

namespace AccessoriesStore.Application.Validators.Inventory;

public class UpdateStockRequestValidator
    : AbstractValidator<UpdateStockRequest>
{
    public UpdateStockRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(
                "Quantity must be greater than zero.");
    }
}