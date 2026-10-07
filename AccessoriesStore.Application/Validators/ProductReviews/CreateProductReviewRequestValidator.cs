using AccessoriesStore.Application.DTOs.ProductReviews;
using FluentValidation;

namespace AccessoriesStore.Application.Validators.ProductReviews
{
    public class CreateProductReviewRequestValidator
    : AbstractValidator<CreateProductReviewRequest>
    {
        public CreateProductReviewRequestValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
                .MaximumLength(1000)
                .WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}
