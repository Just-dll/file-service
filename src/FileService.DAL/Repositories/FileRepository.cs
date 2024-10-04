using FileService.DAL.Data;
using FileService.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly FileServiceDbContext dbContext;
        public FileRepository(FileServiceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void AddFile(Entities.File file)
        {
            dbContext.Add(file);
        }

        public void DeleteFile(Entities.File file)
        {
            dbContext.Remove(file);
        }

        public async Task DeleteFileByIdAsync(uint fileId)
        {
            var foundFile = await dbContext.Files.FindAsync(fileId);
            if (foundFile != null)
            {
                dbContext.Remove(foundFile);
            }
        }

        public Task<bool> FileExistsAsync(string fileName, string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task<Entities.File?> GetFileByIdAsync(uint folderId, uint fileId)
        {
            return await dbContext.Files.Include(f => f.Folder)
                .FirstOrDefaultAsync(f => f.Id == fileId && f.FolderId == folderId);
        }
    }
}
