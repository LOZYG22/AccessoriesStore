using AccessoriesStore.Application.DTOs.Products;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Products
{
    public class CreateProductRequestValidator
    : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Product name is required.")
                .MaximumLength(200)
                .WithMessage("Product name must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Product description is required.")
                .MaximumLength(2000)
                .WithMessage("Product description must not exceed 2000 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(x => x.DiscountPrice)
                .GreaterThan(0)
                .When(x => x.DiscountPrice.HasValue)
                .WithMessage("Discount price must be greater than 0.");

            RuleFor(x => x.SKU)
                .NotEmpty()
                .WithMessage("SKU is required.")
                .MaximumLength(100)
                .WithMessage("SKU must not exceed 100 characters.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stock quantity must be greater than or equal to 0.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Category is required.");

            RuleFor(x => x.DiscountPrice)
                .LessThan(x => x.Price)
                .When(x => x.DiscountPrice.HasValue)
                .WithMessage("Discount price must be less than the original price.");
        }
    }
}
