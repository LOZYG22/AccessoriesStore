using AccessoriesStore.Application.DTOs.Categories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Categories
{
    public class UpdateCategoryRequestValidator
    : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Category name is required.")
                .MaximumLength(100)
                .WithMessage("Category name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .WithMessage("Image URL must not exceed 500 characters.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order cannot be negative.");
        }
    }
}

