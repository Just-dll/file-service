using FileService.DAL.Data;
using FileService.DAL.Interfaces;
using FileService.DAL.Repositories;

namespace FileService.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly FileServiceDbContext dbContext;
        private IFolderRepository? folderRepository;
        private IFileRepository? fileRepository;
        private IUserRepository? userRepository;
        private IUserAccessRepository? userAccessRepository;

        public UnitOfWork(FileServiceDbContext context)
        {
            dbContext = context;
        }

        public IFolderRepository FolderRepository
        {
            get { return folderRepository ??= new FolderRepository(dbContext); }
        }

        public IFileRepository FileRepository
        {
            get { return fileRepository ??= new FileRepository(dbContext); }
        }

        public IUserRepository UserRepository
        {
            get { return userRepository ??= new UserRepository(dbContext); }
        }

        public IUserAccessRepository UserAccessRepository
        {
            get { return userAccessRepository ??= new UserAccessRepository(dbContext); }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

    }

}
