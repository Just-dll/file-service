using Microsoft.EntityFrameworkCore;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;

namespace FileService.DAL.Repositories
{
    public class UserAccessRepository : IUserAccessRepository
    {
        private readonly FileServiceDbContext dbContext;
        public UserAccessRepository(FileServiceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddUserAccess(UserAccess access)
        {
            dbContext.UserAccesses.Add(access);
        }

        public void DeleteUserAccess(UserAccess access)
        {
            dbContext.Remove(access);
        }

        public async Task DeleteUserAccessAsync(uint folderId, Guid userId)
        {
            var accessToDelete = await dbContext.UserAccesses.FindAsync(folderId, userId);
            if (accessToDelete != null && !accessToDelete.AccessFlags.HasFlag(AccessPermission.Owner))
            {
                dbContext.Remove(accessToDelete);
            }
        }

        //public async Task<IEnumerable<UserAccess>> GetFolderAccessors(uint folderId)
        //{
        //    var outerFolders = await dbContext.Folders
        //            .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
        //            .Include(f => f.Accessors)
        //            .ToListAsync();

        //    return outerFolders.SelectMany(f => f.Accessors).DistinctBy(ua => ua.UserId);
        //}

        public async Task<IEnumerable<UserAccess>> GetFolderAccessors(uint folderId)
        {
            // Get the IDs of all outer folders including the main folder
            var outerFolderIds = (await dbContext.Folders
                .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", folderId)
                .ToListAsync())
                .Select(f => f.Id);

            // Retrieve all unique accessors for those folder IDs in a single query
            var accessors = (await dbContext.UserAccesses
                    .Include(ua => ua.User)
                .Where(ua => outerFolderIds.Contains(ua.FolderId))
                .ToListAsync())
                .DistinctBy(ua => ua.UserId);

            return accessors;
        }




        public async Task<UserAccess?> GetUserAccessAsync(uint folderId, Guid userId)
        {
            return await dbContext.UserAccesses
                .Include(f => f.User)
                .Include(f => f.Folder)
                .FirstOrDefaultAsync(f => f.FolderId == folderId && f.User.IdentityGuid == userId);
        }

        public void UpdateUserAccess(UserAccess access)
        {
            dbContext.Entry(access).State = EntityState.Modified;
        }
    }
}
