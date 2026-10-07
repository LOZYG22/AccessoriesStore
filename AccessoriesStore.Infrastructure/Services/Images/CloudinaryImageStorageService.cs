using AccessoriesStore.Application.Abstractions.Images;
using AccessoriesStore.Application.DTOs.Images;
using AccessoriesStore.Infrastructure.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Infrastructure.Services.Images
{
    public class CloudinaryImageStorageService : IImageStorageService
    {
        private readonly CloudinarySettings _settings;

        public CloudinaryImageStorageService(
            IOptions<CloudinarySettings> options)
        {
            _settings = options.Value;
        }

        public async Task<Application.DTOs.Images.ImageUploadResult> UploadAsync(
                    Stream file,
                    string fileName,
                    string contentType)
        {
            var cloudinary = new Cloudinary(
                new Account(
                    _settings.CloudName,
                    _settings.ApiKey,
                    _settings.ApiSecret));

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(
                    fileName,
                    file)
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
            {
                throw new Exception(result.Error.Message);
            }

            return new Application.DTOs.Images.ImageUploadResult
            {
                ImageUrl = result.SecureUrl.ToString(),
                PublicId = result.PublicId
            };
        }

        public async Task DeleteAsync(string publicId)
        {
            var cloudinary = new Cloudinary(
                new Account(
                    _settings.CloudName,
                    _settings.ApiKey,
                    _settings.ApiSecret));

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await cloudinary.DestroyAsync(deleteParams);

            if (result.Error is not null)
            {
                throw new Exception(result.Error.Message);
            }
        }
    }
}
