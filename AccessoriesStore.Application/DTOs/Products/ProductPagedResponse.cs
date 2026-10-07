namespace AccessoriesStore.Application.DTOs.Products
{
    public class ProductPagedResponse
    {
        public List<ProductResponse> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }
}
