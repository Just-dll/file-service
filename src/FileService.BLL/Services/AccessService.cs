using AutoMapper;
using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Services
{
    public class AccessService : IAccessService
    {
        private readonly FileServiceDbContext _dbContext;
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;
        public AccessService(FileServiceDbContext dbContext, IIdentityService identityService, IMapper mapper)
        {
            _dbContext = dbContext;
            _identityService = identityService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AccessModel>> GetAccessors(uint folderId)
        {
            var folderPath = await _dbContext.Folders
                .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
                .ToListAsync();

            var folderPathIds = folderPath.Select(x => x.Id);

            var accessors = await _dbContext.UserAccesses
                .Include(ua => ua.User)
                .Where(ua => folderPathIds.Contains(ua.FolderId))
                .ToListAsync();

            return accessors.DistinctBy(x => x.UserId).Select(ua => _mapper.Map<AccessModel>(ua));
        }

        public async Task<AccessModel> GiveAccess(uint folderId, AccessShortModel model)
        {
            var user = await _identityService.GetUserByEmail(model.Email) ?? throw new KeyNotFoundException();
            var folder = await _dbContext.Folders.FindAsync(folderId) ?? throw new KeyNotFoundException();
            
            var entry = _dbContext.UserAccesses.Add(new() { FolderId = folder.Id, UserId = user.Id, AccessFlags = model.Permission });
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<AccessModel>(entry.Entity);
        }

        public async Task<bool> GetAccessVerification(Guid userId, uint folderId, AccessPermission requiredPermission)
        {
            var folderPath = await _dbContext.Folders
               .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
               .ToListAsync();

            var user = await _dbContext.Users
                .Include(u => u.UserAccesses)
                .FirstOrDefaultAsync(u => u.IdentityGuid == userId) ?? throw new UnauthorizedAccessException();

            return user.UserAccesses
                .Where(ua => folderPath.Select(f => f.Id).Contains(ua.FolderId))
                .Any(ua => ua.AccessFlags.HasFlag(requiredPermission));
        }

        public async Task<AccessModel> UpdateAccess(uint folderId, AccessShortModel model)
        {
            var currAccess = await _dbContext.UserAccesses
                .Include(ua => ua.User)
                .Include(ua => ua.Folder)
                .FirstOrDefaultAsync(ua => ua.FolderId == folderId && ua.User.Email == model.Email) 
                ?? throw new KeyNotFoundException();

            currAccess.AccessFlags = model.Permission;

            _dbContext.Entry(currAccess).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            return _mapper.Map<AccessModel>(currAccess);
        }


        public async Task DeleteAccess(uint folderId, string email)
        {
            var userAccess = _dbContext.UserAccesses
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.User.Email == email && ua.FolderId == folderId);

            if (userAccess != null)
            {
                _dbContext.Remove(userAccess);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<FolderModel>> GetAccessibleFoldersAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserAccesses)
                    .ThenInclude(u => u.Folder)
                .FirstOrDefaultAsync(u => u.IdentityGuid == userId) ?? throw new UnauthorizedAccessException();

            return user.UserAccesses.Select(ua => _mapper.Map<FolderModel>(ua.Folder));
        }
    }
}
