
namespace AccessoriesStore.Application.DTOs.Products
{
    public class ProductRatingResponse
    {
        public double Average { get; set; }

        public int Count { get; set; }

        public int FiveStars { get; set; }

        public int FourStars { get; set; }

        public int ThreeStars { get; set; }

        public int TwoStars { get; set; }

        public int OneStar { get; set; }
    }
}
