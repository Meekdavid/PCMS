using Common.ConfigurationSettings;
using Common.Models;
using Core.Results;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Literals.StringLiterals;

namespace Infrastructure.Repositories
{
    public class LocalStorage : ILocalStorage
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        // Constructor to initialize dependencies
        public LocalStorage(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public async Task<Core.Results.IResult> DeleteAsync(string fpath, string fileName)
        {
            // Check if the file exists and delete it; return an appropriate result
            if (File.Exists(fpath))
            {
                File.Delete(fpath);
                return new SuccessResult("File deleted successfully.");
            }
            return new ErrorResult("File not found.");
        }

        public IDataResult<List<string>> GetFiles(string path)
        {
            // List all files in the specified directory
            DirectoryInfo directory = new(path);
            if (directory.Exists)
            {
                var fileNames = directory.GetFiles().Select(f => f.Name).ToList();
                return new SuccessDataResult<List<string>>(fileNames);
            }
            return new ErrorDataResult<List<string>>(null, ResponseMessage_DirectoryNotFound, ResponseCode_DirectoryNotFound);
        }

        public IDataResult<string> HasFile(string path, string fileName)
        {
            // Check if the specified file exists and return its full path
            string fullPath = Path.Combine(ConfigSettings.ApplicationSetting.BaseLocalStorageDomain, path, fileName);
            bool exists = File.Exists(fullPath);
            return new SuccessDataResult<string>(fullPath);
        }

        public IDataResult<string> HasFileFromDirectlyFullPath(string fullPath)
        {
            // Check if the file exists based on its full path
            bool exists = File.Exists(fullPath);
            string content = exists ? fullPath : null;
            return new SuccessDataResult<string>(fullPath);
        }

        public async Task<IDataResult<(string fileName, string pathOrContainerName)>> SingleUploadAsync(string path, IFormFile file, HttpContext httpContext)
        {
            string fileNewName = String.Empty, fileUrl = String.Empty;
            try
            {
                // Construct the upload path
                string uploadPath = Path.Combine("wwwroot", path);

                // Create the directory if it does not exist
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                fileNewName = file.FileName;
                string fullPath = Path.Combine(uploadPath, fileNewName);
                await CopyFileAsyncLocal(fullPath, file);

                // Generate file URL for download
                string baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}";
                fileUrl = $"{baseUrl}/{path}/{fileNewName}".Replace("\\", "/");

                
            }
            catch (Exception ex)
            {
                // If exception occurs while trying to upload file, do not break the flow of the program
            }

            return new SuccessDataResult<(string fileName, string pathOrContainerName)>((fileNewName, fileUrl));
        }

        private async Task CopyFileAsyncLocal(string destinationPath, IFormFile file)
        {
            // Save the uploaded file to the local storage
            using (var stream = new FileStream(destinationPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        public async Task<IDataResult<List<(string fileName, string pathOrContainerName)>>> MultipleUploadAsync(List<string> paths, List<IFormFile> files)
        {
            // Ensure that the number of paths matches the number of files
            if (paths.Count != files.Count)
            {
                return new ErrorDataResult<List<(string fileName, string pathOrContainerName)>>(null, ResponseCode_Success, ResponseMessage_FilesCountMismatch);
            }

            List<(string fileName, string pathOrContainerName)> uploadedFiles = new();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var path = paths[i];

                // Upload each file to the corresponding path
                string uploadPath = Path.Combine(ConfigSettings.ApplicationSetting.BaseLocalStorageDomain, path);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string fileNewName = file.FileName;
                await CopyFileAsync($"{uploadPath}\\{fileNewName}", file);

                string pathNewName = $"Uploads/{path}/{fileNewName}";
                uploadedFiles.Add((file.FileName, pathNewName));
            }

            return new SuccessDataResult<List<(string fileName, string pathOrContainerName)>>(uploadedFiles);
        }

        public async Task<IDataResult<List<(string fileName, string pathOrContainerName)>>> UploadAsync(string path, IFormFileCollection files)
        {
            // Upload multiple files to the specified path
            string uploadPath = Path.Combine(ConfigSettings.ApplicationSetting.BaseLocalStorageDomain, path);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            List<(string fileName, string path)> datas = new();

            foreach (IFormFile file in files)
            {
                string fileNewName = file.FileName;
                await CopyFileAsync($"{uploadPath}\\{fileNewName}", file);

                string pathNewName = $"{path}/{fileNewName}";
                datas.Add((file.FileName, pathNewName));
            }

            return new SuccessDataResult<List<(string fileName, string pathOrContainerName)>>(datas);
        }

        private async Task<bool> CopyFileAsync(string path, IFormFile file)
        {
            // Copy file to the specified path
            try
            {
                await using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: true);
                await file.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("File upload failed.", ex);
            }
        }

        public async Task<Core.Results.IResult> DeleteFromDirectlyFullPaths(List<string> fullPaths)
        {
            // Delete multiple files based on their full paths
            foreach (var fullPath in fullPaths)
            {
                var fileFullPath = Path.Combine(ConfigSettings.ApplicationSetting.BaseLocalStorageDomain, fullPath);
                if (File.Exists(fileFullPath))
                {
                    File.Delete(fileFullPath);
                }
            }

            return new SuccessResult("Files deleted successfully.");
        }

        public async Task<Core.Results.IResult> DeleteFromDirectlyFullPath(string fullPath)
        {
            // Delete a single file based on its full path
            string fileFullPath = Path.Combine(ConfigSettings.ApplicationSetting.BaseLocalStorageDomain, fullPath);
            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
                return new SuccessResult("File deleted successfully.");
            }
            return new ErrorResult("File not found.");
        }
    }


}
