using FileService.DAL.Entities;

namespace FileService.DAL.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByEmail(string email);
        void AddUser(User user);
    }
}
