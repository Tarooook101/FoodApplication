using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Services
{
    public class FileService : IFileService
    {
        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int maxFileSizeInMB = 5;

        public async Task<string> SaveImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was provided");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only jpg, jpeg, png, and gif are allowed.");

            if (file.Length > maxFileSizeInMB * 1024 * 1024)
                throw new ArgumentException($"File size exceeds {maxFileSizeInMB}MB limit.");

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(webRootPath, "images", "recipes");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("images", "recipes", uniqueFileName);
        }

        public void DeleteImage(string imagePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(webRootPath, imagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file, string webRootPath);
        void DeleteImage(string imagePath, string webRootPath);
    }
}
