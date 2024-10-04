namespace FileService.DAL.Interfaces
{
    public interface IFileRepository
    {
        Task<Entities.File?> GetFileByIdAsync(uint folderId, uint fileId);
        Task<bool> FileExistsAsync(string fileName, string filePath);
        void AddFile(Entities.File file);
        void DeleteFile(Entities.File file);
        Task DeleteFileByIdAsync(uint fileId);
    }
}