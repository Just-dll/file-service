using AutoMapper;
using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Threading;

namespace FileService.BLL.Services
{
    public class FolderService : IFolderService
    {
        private readonly FileServiceDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStorageProvider _storageProvider;
        public FolderService(FileServiceDbContext context, IMapper mapper, IStorageProvider provider)
        {
            _context = context;
            _mapper = mapper;
            _storageProvider = provider;
        }

        public async Task<FolderModel?> GetFolderAsync(uint folderId)
        {
            var folder = await _context.Folders
                .Include(f => f.InnerFolders)
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.Id == folderId);
            return _mapper.Map<FolderModel?>(folder);
        }

        public async Task<FolderModel> CreateFolderAsync(string name, uint? parentFolderId)
        {
            var folder = new Folder
            {
                Name = name,
                FolderId = parentFolderId,
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            return _mapper.Map<FolderModel>(folder);
        }

        public async Task DeleteFolderAsync(uint folderId)
        {
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder != null)
            {
                var folderPath = await GetFullFolderPathAsync(folderId);

                var fullPath = string.Join('/', folderPath.Select(f => f.Name));
                await _storageProvider.DeleteItemAsync(fullPath);

                _context.Folders.Remove(folder);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateFolderAsync(uint folderId, FolderModel folder)
        {
            try
            {
                var folderPath = (await GetFullFolderPathAsync(folderId)).Select(f => f.Name).ToList();

                var fullPath = string.Join('/', folderPath);
                folderPath[^1] = folder.Name; 
                var dest = string.Join('/', folderPath);

                await _storageProvider.UpdateFolderAsync(fullPath, dest);

                var folderEntity = await _context.Folders.FindAsync(folderId) ?? throw new KeyNotFoundException();
                _mapper.Map(folder, folderEntity);

                _context.Entry(folderEntity).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                //_context.Database.CommitTransaction();
            }
            catch (Exception ex)
            {
                //_context.Database.RollbackTransaction();
                throw;
            }
        }

        public async Task<FolderArchiveModel?> GetFolderArchiveAsync(uint folderId)
        {
            // Fetch the folder entity (root folder)
            var folder = await _context.Folders
                .Include(f => f.InnerFolders)
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder == null)
            {
                return null;
            }

            // Get full folder path from root downwards
            var folderPath = await GetFullFolderPathAsync(folderId); // Use existing method to build folder path
            var relativePath = string.Join('/', folderPath.Select(f => f.Name));

            // Zip the folder using the new IStorageProvider.ReadFolderAsync
            var archiveData = await _storageProvider.ReadFolderAsync(relativePath);

            // Return the result as FolderArchiveModel
            return new FolderArchiveModel
            {
                FolderName = folder.Name,
                ArchiveData = archiveData
            };
        }

        //private async Task AddFolderToArchive(Folder folder, string relativePath, ZipArchive zipArchive)
        //{
        //    // Add all files in this folder to the zip archive
        //    foreach (var file in folder.Files)
        //    {
        //        var fileBytes = await _storageProvider.ReadFileAsync(relativePath, file.Name);
        //        var entry = zipArchive.CreateEntry(Path.Combine(relativePath, file.Name));

        //        using (var entryStream = entry.Open())
        //        {
        //            await entryStream.WriteAsync(fileBytes, 0, fileBytes.Length);
        //        }
        //    }

        //    // Recursively process all inner folders
        //    foreach (var innerFolder in folder.InnerFolders)
        //    {
        //        var innerFolderPath = Path.Combine(relativePath, innerFolder.Name);
        //        // Recursively add inner folder contents
        //        await AddFolderToArchive(innerFolder, innerFolderPath, zipArchive);
        //    }
        //}

        public async Task<IEnumerable<FolderShortModel>> GetFullFolderPathAsync(uint folderId)
        {
            var folderPath = await _context.Folders
                   .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
                   .ToListAsync();

            if (folderPath.Count < 1)
            {
                throw new DirectoryNotFoundException();
            }

            return folderPath.Select(f => _mapper.Map<FolderShortModel>(f));
        }

    }
}
