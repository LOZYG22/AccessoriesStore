using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using AccessoriesStore.Infrastructure.Services.Common;
using AccessoriesStore.Infrastructure.Services.Products;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Tests
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task GetByIdAsync_ProductDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var slugGenerator = new SlugGenerator();

            var service = new ProductService(
                context,
                slugGenerator);

            // Act
            var action = () => service.GetByIdAsync(999);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(action);
        }
    }
}