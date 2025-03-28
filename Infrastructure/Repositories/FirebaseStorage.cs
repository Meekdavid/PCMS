using Common.ConfigurationSettings;
using Common.Models;
using Core.Results;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client.Extensions.Msal;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Literals.StringLiterals;

namespace Infrastructure.Repositories
{
    public class FirebaseStorage : IFirebaseStorage
    {
        private readonly string _bucketName = ConfigSettings.ApplicationSetting.FireBaseStorage.BucketName;
        private readonly string _baseUrl = ConfigSettings.ApplicationSetting.FireBaseStorage.BaseUrl;
        private readonly StorageClient _storageClient;

        public FirebaseStorage()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("serviceAccountKey.json")
                });
            }

            _storageClient = StorageClient.Create();
        }

        public async Task<Core.Results.IResult> DeleteAsync(string path, string fileName)
        {
            try
            {
                // Attempt to delete the specified file from the storage.
                await _storageClient.DeleteObjectAsync(_bucketName, $"{path}/{fileName}");
                return new SuccessResult("File deleted successfully.");
            }
            catch (Exception)
            {
                // Return an error if the file cannot be found or deletion fails.
                return new ErrorResult("File not found or deletion failed.");
            }
        }

        public IDataResult<List<string>> GetFiles(string path)
        {
            // Retrieve a list of file names from the specified path in the storage.
            var files = _storageClient.ListObjects(_bucketName, path)
                                      .Select(obj => obj.Name)
                                      .ToList();

            // Return the list of files, or an error result if no files are found.
            return files.Any()
                ? new SuccessDataResult<List<string>>(files)
                : new ErrorDataResult<List<string>>(null, ResponseCode_FilesNotFound, ResponseMessage_FilesNotFound);
        }

        public IDataResult<string> HasFile(string path, string fileName)
        {
            var filePath = $"{path}/{fileName}";

            // Check if the specified file exists in the storage.
            var file = _storageClient.ListObjects(_bucketName, filePath).FirstOrDefault();
            return file != null
                ? new SuccessDataResult<string>(filePath)
                : new ErrorDataResult<string>(null, ResponseCode_FilesNotFound, ResponseMessage_FilesNotFound);
        }

        public async Task<IDataResult<(string fileName, string pathOrContainerName)>> SingleUploadAsync(string path, IFormFile file, HttpContext httpContext)
        {
            string fileName = file.FileName;
            string filePath = $"{path}/{fileName}";

            // Upload a single file to the storage.
            using var stream = file.OpenReadStream();
            await _storageClient.UploadObjectAsync(_bucketName, filePath, file.ContentType, stream);

            // Generate the download URL for the uploaded file.
            string downloadUrl = $"{_baseUrl}{_bucketName}/o/{Uri.EscapeDataString(filePath)}?alt=media";

            return new SuccessDataResult<(string fileName, string pathOrContainerName)>((fileName, downloadUrl));
        }

        public async Task<IDataResult<List<(string fileName, string pathOrContainerName)>>> MultipleUploadAsync(List<string> paths, List<IFormFile> files)
        {
            if (paths.Count != files.Count)
                // Ensure the number of paths matches the number of files before proceeding.
                return new ErrorDataResult<List<(string fileName, string pathOrContainerName)>>(null, ResponseCode_FilesCountMismatch, ResponseMessage_FilesCountMismatch);

            var uploadedFiles = new List<(string fileName, string pathOrContainerName)>();

            for (int i = 0; i < files.Count; i++)
            {
                string fileName = files[i].FileName;
                string filePath = $"{paths[i]}/{fileName}";

                // Upload each file and store its details.
                using var stream = files[i].OpenReadStream();
                await _storageClient.UploadObjectAsync(_bucketName, filePath, files[i].ContentType, stream);

                string downloadUrl = $"{_baseUrl}{_bucketName}/o/{Uri.EscapeDataString(filePath)}?alt=media";
                uploadedFiles.Add((fileName, downloadUrl));
            }

            // Return a list of successfully uploaded files.
            return new SuccessDataResult<List<(string fileName, string pathOrContainerName)>>(uploadedFiles);
        }

        public async Task<Core.Results.IResult> DeleteFromDirectlyFullPaths(List<string> fullPaths)
        {
            foreach (var filePath in fullPaths)
            {
                try
                {
                    // Attempt to delete each file from its full path.
                    await _storageClient.DeleteObjectAsync(_bucketName, filePath);
                }
                catch (Exception)
                {
                    // Return an error if any file deletion fails.
                    return new ErrorResult($"Failed to delete {filePath}.");
                }
            }
            return new SuccessResult("Files deleted successfully.");
        }

        public async Task<Core.Results.IResult> DeleteFromDirectlyFullPath(string fullPath)
        {
            try
            {
                // Attempt to delete a file using its full path.
                await _storageClient.DeleteObjectAsync(_bucketName, fullPath);
                return new SuccessResult("File deleted successfully.");
            }
            catch (Exception)
            {
                // Return an error if the file cannot be found.
                return new ErrorResult("File not found.");
            }
        }

        public Task<IDataResult<List<(string fileName, string pathOrContainerName)>>> UploadAsync(string pathOrContainerName, IFormFileCollection files)
        {
            // Not yet implemented.
            throw new NotImplementedException();
        }

        public IDataResult<string> HasFileFromDirectlyFullPath(string fullPath)
        {
            // Not yet implemented.
            throw new NotImplementedException();
        }

    }

}
