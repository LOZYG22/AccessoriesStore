using AccessoriesStore.Application.DTOs.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Orders
{
    public class UpdateOrderStatusRequestValidator
        : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status is required.");
        }

    }
}
