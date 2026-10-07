using AccessoriesStore.Application.DTOs.Carts;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Carts
{
    public class AddToCartRequestValidator
    : AbstractValidator<AddToCartRequest>
    {
        public AddToCartRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("ProductId must be greater than zero.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        }
    }
}
