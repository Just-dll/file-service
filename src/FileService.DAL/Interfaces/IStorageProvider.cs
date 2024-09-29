using FileService.DAL.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Interfaces
{
    /// <summary>
    /// Implementation of Repository pattern on file storage
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Asynchronous loading of <paramref name="formFile"/> into the provided storage by <paramref name="relativePath"/>
        /// </summary>
        /// <param name="relativePath">Relative path of file</param>
        /// <param name="formFile">Uploaded file</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Internal path of file</returns>
        Task<string> UploadFileAsync(string relativePath, IFormFile formFile, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously reads a file from the specified <paramref name="relativePath"/> 
        /// using the provided <paramref name="fileName"/>
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> ReadFileAsync(string relativePath, string fileName, CancellationToken cancellationToken = default);
        Task<byte[]> ReadFolderAsync(string relativePath, CancellationToken cancellationToken = default);
        Task DeleteItemAsync(string relativePath, string? filename = default, CancellationToken cancellation = default);
        
        Task UpdateFolderAsync(string currentRelativePath, string destination, CancellationToken cancellationToken = default);
    }
}
