using Common.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.General
{
    /// <summary>
    /// Defines an interface for storage operations, including file uploads, deletions, and retrieval.
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Asynchronously uploads a collection of files to the specified path or container.
        /// </summary>
        /// <param name="pathOrContainerName">The path or container name where the files should be uploaded.</param>
        /// <param name="files">The collection of files to upload.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing a list of file names and their corresponding paths or container names.</returns>
        Task<IDataResult<List<(string fileName, string pathOrContainerName)>>> UploadAsync(string pathOrContainerName, IFormFileCollection files);

        /// <summary>
        /// Asynchronously uploads a single file to the specified path or container.
        /// </summary>
        /// <param name="pathOrContainerName">The path or container name where the file should be uploaded.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="httpContext">The HttpContext for use during upload operations.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing the file name and its corresponding path or container name.</returns>
        Task<IDataResult<(string fileName, string pathOrContainerName)>> SingleUploadAsync(string pathOrContainerName, IFormFile file, HttpContext httpContext);

        /// <summary>
        /// Asynchronously uploads a list of files to respective paths.
        /// </summary>
        /// <param name="paths">List of paths to which files are to be uploaded.</param>
        /// <param name="files">List of files to be uploaded.</param>
        /// <returns>A task representing the asynchronous operation, returning a data result containing a list of file names and their corresponding paths or container names.</returns>
        Task<IDataResult<List<(string fileName, string pathOrContainerName)>>> MultipleUploadAsync(List<string> paths, List<IFormFile> files);

        /// <summary>
        /// Asynchronously deletes a file from the specified path or container.
        /// </summary>
        /// <param name="pathOrContainerName">The path or container name where the file is located.</param>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>A task representing the asynchronous operation, returning a result indicating success or failure.</returns>
        Task<Core.Results.IResult> DeleteAsync(string pathOrContainerName, string fileName);

        /// <summary>
        /// Asynchronously deletes a file from the specified full path.
        /// </summary>
        /// <param name="fullPath">The full path of the file to delete.</param>
        /// <returns>A task representing the asynchronous operation, returning a result indicating success or failure.</returns>
        Task<Core.Results.IResult> DeleteFromDirectlyFullPath(string fullPath);

        /// <summary>
        /// Asynchronously deletes multiple files from the specified full paths.
        /// </summary>
        /// <param name="fullPaths">List of full paths of files to be deleted.</param>
        /// <returns>A task representing the asynchronous operation, returning a result indicating success or failure.</returns>
        Task<Core.Results.IResult> DeleteFromDirectlyFullPaths(List<string> fullPaths);

        /// <summary>
        /// Retrieves a list of file names from the specified path or container.
        /// </summary>
        /// <param name="pathOrContainerName">The path or container name to retrieve files from.</param>
        /// <returns>A data result containing a list of file names.</returns>
        IDataResult<List<string>> GetFiles(string pathOrContainerName);

        /// <summary>
        /// Checks if a file exists in the specified path or container.
        /// </summary>
        /// <param name="pathOrContainerName">The path or container name to check.</param>
        /// <param name="fileName">The name of the file to check for.</param>
        /// <returns>A data result containing a string indicating the result of the existence check.</returns>
        IDataResult<string> HasFile(string pathOrContainerName, string fileName);

        /// <summary>
        /// Checks if a file exists at the specified full path.
        /// </summary>
        /// <param name="fullPath">The full path of the file to check.</param>
        /// <returns>A data result containing a string indicating the result of the existence check.</returns>
        IDataResult<string> HasFileFromDirectlyFullPath(string fullPath);
    }
}
