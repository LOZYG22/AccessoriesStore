using AccessoriesStore.Application.DTOs.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Orders
{
    public class ConfirmOrderRequestValidator
        : AbstractValidator<ConfirmOrderRequest>
    {
        public ConfirmOrderRequestValidator()
        {
            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Shipping cost cannot be negative.");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Discount cannot be negative.");
        }
    }
}
