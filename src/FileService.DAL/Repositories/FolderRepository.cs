using Microsoft.EntityFrameworkCore;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly FileServiceDbContext dbContext;
        public FolderRepository(FileServiceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Folder?> GetFolderByIdAsync(uint id)
        {
            return await dbContext.Folders
                .Include(f => f.Files)
                .Include(f => f.InnerFolders)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Folder>> GetOuterFoldersAsync(uint id)
        {
            return await dbContext.Folders
                .FromSqlRaw("EXEC LoadAllOuterFolders @FolderId = {0}", id)
                .ToListAsync();
        }

        public void CreateFolder(Folder folder)
        {
            dbContext.Folders.Add(folder);
        }

        public void UpdateFolder(Folder folder)
        {
            dbContext.Entry(folder).State = EntityState.Modified;
        }

        public void DeleteFolder(Folder folder)
        {
            dbContext.Remove(folder);
        }

        public async Task DeleteFolderAsync(uint id)
        {
            var folder = await dbContext.FindAsync<Folder>(id);
            if (folder != null)
            {
                dbContext.Remove(folder);
            }
        }
    }
}
