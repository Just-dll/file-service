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
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStorageProvider _storageProvider;
        public FolderService(IUnitOfWork unitOfWork, IMapper mapper, IStorageProvider provider)
        {
            _mapper = mapper;
            _storageProvider = provider;
            this.unitOfWork = unitOfWork;
        }

        public async Task<FolderModel?> GetFolderAsync(uint folderId)
        {
            var folder = await unitOfWork.FolderRepository.GetFolderByIdAsync(folderId);
            return _mapper.Map<FolderModel?>(folder);
        }

        public async Task<FolderModel> CreateFolderAsync(string name, uint? parentFolderId)
        {
            var folder = new Folder
            {
                Name = name,
                FolderId = parentFolderId,
            };

            unitOfWork.FolderRepository.CreateFolder(folder);

            await unitOfWork.SaveChangesAsync();

            return _mapper.Map<FolderModel>(folder);
        }

        public async Task DeleteFolderAsync(uint folderId)
        {
            //var folder = await _context.Folders.FindAsync(folderId);
            var folder = await unitOfWork.FolderRepository.GetFolderByIdAsync(folderId);
            if (folder != null)
            {
                var folderPath = await unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId);

                var fullPath = string.Join('/', folderPath.Select(f => f.Name));
                await _storageProvider.DeleteItemAsync(fullPath);

                unitOfWork.FolderRepository.DeleteFolder(folder);
                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpdateFolderAsync(uint folderId, FolderModel folder)
        {
            try
            {
                var folderEntity = await unitOfWork.FolderRepository.GetFolderByIdAsync(folderId) ?? throw new KeyNotFoundException();
                var folderPath = (await unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId)).Select(f => f.Name).ToList();

                var fullPath = string.Join('/', folderPath);
                folderPath[^1] = folder.Name; 
                var dest = string.Join('/', folderPath);

                await _storageProvider.UpdateFolderAsync(fullPath, dest);

                _mapper.Map(folder, folderEntity);

                unitOfWork.FolderRepository.UpdateFolder(folderEntity);

                await unitOfWork.SaveChangesAsync();
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
            var folder = await unitOfWork.FolderRepository.GetFolderByIdAsync(folderId);

            if (folder == null)
            {
                return null;
            }

            var folderPath = await unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId);

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

        public async Task<IEnumerable<FolderShortModel>> GetFullFolderPathAsync(uint folderId)
        {
            var folderPath = await unitOfWork.FolderRepository.GetOuterFoldersAsync(folderId);

            if (!folderPath.Any())
            {
                throw new DirectoryNotFoundException();
            }

            return folderPath.Select(f => _mapper.Map<FolderShortModel>(f));
        }

    }
}
