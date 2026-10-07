using AccessoriesStore.Application.DTOs.Carts;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Carts
{
    public class UpdateCartItemRequestValidator
    : AbstractValidator<UpdateCartItemRequest>
    {
        public UpdateCartItemRequestValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        }
    }
}
