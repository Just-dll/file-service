using AutoMapper;
using Azure.Core;
using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
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
                var folderPath = await _context.Folders
                   .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
                   .ToListAsync();

                if (folderPath == null || folderPath.Count < 1)
                {
                    throw new InvalidOperationException();
                }

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

                //_context.Database.BeginTransaction();
                var folderPath = await _context.Folders
               .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
               .ToListAsync();

                if (folderPath == null || folderPath.Count < 1)
                {
                    throw new InvalidDataException();
                }

                var fullPath = string.Join('/', folderPath.Select(f => f.Name));

                folderPath[^1].Name = folder.Name;

                var dest = string.Join('/', folderPath.Select(f => f.Name));

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
    }
}
