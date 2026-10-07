using AccessoriesStore.Application.DTOs.Images;

namespace AccessoriesStore.Application.Abstractions.Images
{
    public interface IImageStorageService
    {
        Task<ImageUploadResult> UploadAsync(
            Stream file,
            string fileName,
            string contentType);

        Task DeleteAsync(string publicId);
    }
}
