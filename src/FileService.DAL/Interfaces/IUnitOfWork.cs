namespace FileService.DAL.Interfaces
{
    public  interface IUnitOfWork
    {
        IFolderRepository FolderRepository { get; }
        IUserAccessRepository UserAccessRepository { get; }
        IUserRepository UserRepository { get; }
        IFileRepository FileRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
