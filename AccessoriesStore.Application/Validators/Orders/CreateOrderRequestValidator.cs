using AccessoriesStore.Application.DTOs.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Orders
{
    public class CreateOrderRequestValidator
        : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.AddressId)
                .GreaterThan(0)
                .WithMessage("AddressId must be greater than zero.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes))
                .WithMessage("Notes must not exceed 500 characters.");
        }
    }
}
