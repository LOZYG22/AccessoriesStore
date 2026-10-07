using AccessoriesStore.Application.DTOs.Addresses;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Addresses
{
    public class UpdateAddressRequestValidator
    : AbstractValidator<UpdateAddressRequest>
    {
        public UpdateAddressRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.City)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Area)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Street)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.BuildingNumber)
                .MaximumLength(20);

            RuleFor(x => x.ApartmentNumber)
                .MaximumLength(20);

            RuleFor(x => x.AdditionalDetails)
                .MaximumLength(500);
        }
    }
}
