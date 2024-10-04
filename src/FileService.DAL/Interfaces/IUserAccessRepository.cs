using FileService.DAL.Entities;

namespace FileService.DAL.Interfaces
{
    public interface IUserAccessRepository
    {
        Task<UserAccess?> GetUserAccessAsync(uint folderId, Guid userId);
        Task<IEnumerable<UserAccess>> GetFolderAccessors(uint folderId);
        void AddUserAccess(UserAccess access);
        void UpdateUserAccess(UserAccess access);
        void DeleteUserAccess(UserAccess access);
        Task DeleteUserAccessAsync(uint folderId, Guid userId);
    }

}
