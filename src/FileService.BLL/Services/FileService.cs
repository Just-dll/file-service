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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Services
{
    public class FileService : IFileService
    {
        private readonly FileServiceDbContext _dbContext;
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;

        public FileService(FileServiceDbContext context, IStorageProvider storageProvider, IMapper mapper)
        {
            _dbContext = context;
            _storageProvider = storageProvider;
            _mapper = mapper;
        }

        public async Task<FileModel?> GetFileAsync(uint folderId, uint fileId)
        {
            return _mapper.Map<FileModel?>(await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.FolderId == folderId));
        }

        public async Task<FileShortModel> UploadFileAsync(IFormFile file, string filePath)
        {
            if (await _dbContext.Files.AnyAsync(f => f.Name == file.FileName && EF.Functions.Like(f.InternalFilePath, $"%{filePath}")))
            {
                throw new AlreadyExistsException($"The file '{file.FileName}' already exists in the specified path.");
            }

            string intFilePath = await _storageProvider.UploadFileAsync(filePath, file);

            var folderNames = filePath.Split([Path.PathSeparator, Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar], StringSplitOptions.RemoveEmptyEntries);

            // Start with the root folder (assuming folderNames[0] is the root)
            Folder? parentFolder = null;
            Folder? currentFolder = null;

            foreach (var folderName in folderNames)
            {
                currentFolder = await _dbContext.Folders
                    .Where(f => f.Name == folderName && f.OuterFolder == parentFolder)
                    .FirstOrDefaultAsync();

                if (currentFolder == null)
                {
                    // Create the folder if it doesn't exist
                    currentFolder = new Folder
                    {
                        Name = folderName,
                        OuterFolder = parentFolder
                    };

                    _dbContext.Folders.Add(currentFolder);
                    await _dbContext.SaveChangesAsync();
                }

                // Move to the next level
                parentFolder = currentFolder;
            }

            // currentFolder now represents the innermost folder
            if (currentFolder == null)
            {
                throw new InvalidOperationException("Folder structure creation failed.");
            }

            // Save file metadata to the database
            var dbFile = new DAL.Entities.File
            {
                Name = file.FileName,
                InternalFilePath = intFilePath,
                FolderId = currentFolder.Id,
                Size = file.Length,
                CreationDate = DateTime.UtcNow
            };

            _dbContext.Files.Add(dbFile);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<FileShortModel>(dbFile);
        }

        public async Task DeleteFileAsync(uint folderId, uint fileId)
        {
            var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.FolderId == folderId);
            if (file != null)
            {
                // Delete the file from disk
                await _storageProvider.DeleteItemAsync(file.InternalFilePath, file.Name);

                // Remove the file metadata from the database
                _dbContext.Files.Remove(file);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<FileDownloadModel?> DownloadFile(string filePath, string fileName)
        {
            var fileBytes = await _storageProvider.ReadFileAsync(filePath, fileName);
            if (fileBytes == null)
            {
                return null;
            }
            return new FileDownloadModel { Data = fileBytes, Name = fileName };
        }

        public Task DeleteFileAsync(string filePath, string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task<FileDownloadModel?> DownloadFile(uint folderId, uint fileId)
        {
            var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.FolderId == folderId);

            if (file == null)
            {
                return null;
            }

            var folderPath = await _dbContext.Folders
               .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", file.FolderId)
               .ToListAsync();

            var fullPath = string.Join('/', folderPath.Select(f => f.Name));

            var bytes = await _storageProvider.ReadFileAsync(fullPath, file.Name);

            if (bytes == null)
            {
                return null;
            }

            return new FileDownloadModel { Data = bytes, Name = file.Name };
        }

        public Task DeleteFileAsync(uint fileId)
        {
            throw new NotImplementedException();
        }

        public async Task<FileShortModel> UploadFileAsync(IFormFile file, uint folderId)
        {
            var folderPath = await _dbContext.Folders
               .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
               .ToListAsync();

            if (folderPath == null || folderPath.Count < 0)
            {
                throw new DirectoryNotFoundException();
            }

            var fullRelativePath = string.Join('/', folderPath.Select(f => f.Name));

            var intPath = await _storageProvider.UploadFileAsync(fullRelativePath, file);

            var dbFile = new DAL.Entities.File
            {
                Name = file.FileName,
                InternalFilePath = intPath,
                FolderId = folderId,
                Size = file.Length,
                CreationDate = DateTime.UtcNow
            };

            _dbContext.Files.Add(dbFile);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<FileShortModel>(dbFile);
        }
    }

}
