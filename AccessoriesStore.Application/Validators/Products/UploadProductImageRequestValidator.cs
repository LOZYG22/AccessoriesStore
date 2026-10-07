using AccessoriesStore.Application.DTOs.Products;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Validators.Products
{
    public class UploadProductImageRequestValidator
    : AbstractValidator<UploadProductImageRequest>
    {
        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private const long MaxFileSize = 5 * 1024 * 1024;

        public UploadProductImageRequestValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .Must(file => file != Stream.Null)
                .WithMessage("Image file is required.");

            RuleFor(x => x.File)
                .Must(file => file == Stream.Null || file.Length <= MaxFileSize)
                .WithMessage("Image size must not exceed 5 MB.");

            RuleFor(x => x.ContentType)
                .Must(type => AllowedContentTypes.Contains(type))
                .WithMessage("Only JPEG, PNG, and WebP images are allowed.");

            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");
        }
    }
}
