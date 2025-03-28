using Common.ConfigurationSettings;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services.ExtensionServices
{
    public static class IFormFileExtensions
    {
        // Logic to check validity of supplied file. File must be an image and size must be below 1MB
        public static bool BeAValidImage(this IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension) && file.Length <= ConfigSettings.ApplicationSetting.MaximumFileSizeUpload;
        }
    }
}
