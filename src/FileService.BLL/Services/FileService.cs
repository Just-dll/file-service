using AutoMapper;
using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Exceptions;
using FileService.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Services
{
    public class FileService : IFileService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeService _dateTimeService;
        //private static readonly ReadOnlyDictionary<string, string> MimeTypes = new(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        //{
        //    { ".jpg", "image/jpeg" },
        //    { ".jpeg", "image/jpeg" },
        //    { ".png", "image/png" },
        //    { ".pdf", "application/pdf" },
        //    { ".txt", "text/plain" }
        //});

        public FileService(IUnitOfWork unitOfWork, IStorageProvider storageProvider, IMapper mapper, IDateTimeService dateTimeService)
        {
            _unitOfWork = unitOfWork;
            _storageProvider = storageProvider;
            _mapper = mapper;
            _dateTimeService = dateTimeService;
        }

        public async Task<FileModel?> GetFileAsync(uint folderId, uint fileId)
        {
            return _mapper.Map<FileModel?>(await _unitOfWork.FileRepository.GetFileByIdAsync(folderId, fileId));
        }

        public async Task DeleteFileAsync(uint folderId, uint fileId)
        {
            var file = await _unitOfWork.FileRepository.GetFileByIdAsync(folderId, fileId);
            if (file != null)
            {
                // Delete the file from disk
                await _storageProvider.DeleteItemAsync(file.InternalFilePath, file.Name);

                // Remove the file metadata from the database
                _unitOfWork.FileRepository.DeleteFile(file);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<FileDownloadModel?> DownloadFile(uint folderId, uint fileId)
        {
            var file = await _unitOfWork.FileRepository.GetFileByIdAsync(folderId, fileId);

            if (file == null)
            {
                return null;
            }

            var folderPath = await _unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId);

            var fullPath = string.Join('/', folderPath.Select(f => f.Name));

            var bytes = await _storageProvider.ReadFileAsync(fullPath, file.Name);

            return new FileDownloadModel { Data = bytes, Name = file.Name };
        }

        public async Task<FileShortModel> UploadFileAsync(IFormFile file, uint folderId)
        {
            var folder = await _unitOfWork.FolderRepository.GetFolderByIdAsync(folderId) ?? throw new DirectoryNotFoundException();

            if (folder.Files.Any(f => f.Name == file.Name))
            {
                throw new AlreadyExistsException(file.Name);
            }


            var folderPath = await _unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId);

            var fullRelativePath = string.Join('/', folderPath.Select(f => f.Name));

            string intPath = await _storageProvider.UploadFileAsync(fullRelativePath, file);

            var dbFile = new DAL.Entities.File
            {
                Name = file.FileName,
                InternalFilePath = intPath,
                FolderId = folderId,
                Size = file.Length,
                CreationDate = _dateTimeService.DateTimeNow,
            };

            _unitOfWork.FileRepository.AddFile(dbFile);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<FileShortModel>(dbFile);
        }

        public async Task<FilePreviewModel?> GetFilePreviewAsync(uint folderId, uint fileId)
        {
            var file = await _unitOfWork.FileRepository.GetFileByIdAsync(folderId, fileId);
            if (file == null)
            {
                return null;
            }

            var folderPath = await _unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId);

            var fullPath = string.Join('/', folderPath.Select(f => f.Name));

            // Get MIME type
            string mimeType = GetMimeType(file.Name);

            // Return the preview depending on the MIME type
            var fileBytes = await _storageProvider.ReadFileAsync(fullPath, file.Name);

            // You can apply different logic here to limit the size or return thumbnails/previews for certain types.
            // E.g., for images, return a scaled-down version, or for PDF, return the first page.

            return new FilePreviewModel
            {
                Data = fileBytes,
                Name = file.Name,
                ContentType = mimeType
            };
        }

        private static string GetMimeType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                _ => "application/octet-stream", // Default for unknown types
            };
        }
    }
}
