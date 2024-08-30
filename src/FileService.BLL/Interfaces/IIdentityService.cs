using FileService.DAL.Entities;

namespace FileService.BLL.Interfaces
{
    public interface IIdentityService
    {
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByEmail(string email);
    }
}