using FileService.DAL.Entities;

namespace FileService.DAL.Interfaces
{
    public interface IFolderRepository
    {
        Task<Folder?> GetFolderByIdAsync(uint id);
        void CreateFolder(Folder folder);
        Task DeleteFolderAsync(uint id);
        void DeleteFolder(Folder folder);
        void UpdateFolder(Folder folder);
        Task<IEnumerable<Folder>> GetOuterFoldersAsync(uint id); 
    }
}
