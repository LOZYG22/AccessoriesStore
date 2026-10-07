
namespace AccessoriesStore.Application.DTOs.Products
{
    public class ProductFilterRequest
    {
        public string? Search { get; set; }

        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public bool? InStock { get; set; }

        public bool? IsFeatured { get; set; }

        public string? SortBy { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 12;
    }
}
