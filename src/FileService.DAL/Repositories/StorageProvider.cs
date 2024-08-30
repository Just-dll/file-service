using FileService.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Repositories
{
    public class StorageProvider : IStorageProvider
    {
        private readonly string _basicFolder;
        public StorageProvider(string standartFolder)
        {
            _basicFolder = standartFolder;
        }

        public async Task<string> UploadFileAsync(string relativePath, IFormFile formFile, CancellationToken cancellationToken = default)
        {
            var combinedPath = Path.Combine(_basicFolder, relativePath);
            Directory.CreateDirectory(combinedPath);
            var filePath = Path.Combine(combinedPath, formFile.FileName);
            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await formFile.CopyToAsync(stream, cancellationToken);
            return combinedPath;
        }

        public async Task<byte[]?> ReadFileAsync(string relativePath, string fileName, CancellationToken cancellationToken = default)
        {
            string filePath = Path.Combine(_basicFolder, relativePath, fileName);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, cancellationToken);

            return buffer;
        }

        public Task DeleteItemAsync(string relativePath, string? fileName = default, CancellationToken cancellation = default)
        {
            if (cancellation.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellation);
            }

            string itemPath = Path.Combine(_basicFolder, relativePath);

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    if (Directory.Exists(itemPath))
                    {
                        Directory.Delete(itemPath, true);
                    }
                }
                else
                {
                    string filePath = Path.Combine(itemPath, fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return Task.FromException(ex);
            }

            return Task.CompletedTask;
        }


        public Task UpdateFolderAsync(string currentRelativePath, string destination, CancellationToken cancellationToken = default)
        {
            string intPathCurr = Path.Combine(_basicFolder, currentRelativePath);

            string intPathDest = Path.Combine(_basicFolder, destination);

            if (Directory.Exists(intPathCurr) && intPathCurr != intPathDest)
            {
                Directory.Move(intPathCurr, intPathDest);
            }

            return Task.CompletedTask;
        }
    }
}
