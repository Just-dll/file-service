using FileService.DAL.Data;
using FileService.DAL.Entities;
using FileService.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FileServiceDbContext dbContext;
        public UserRepository(FileServiceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddUser(User user)
        {
            dbContext.Users.Add(user);
        }

        public Task<User?> GetUserByEmail(string email)
        {
            return dbContext.Users
                .Include(u => u.UserAccesses)
                    .ThenInclude(ua => ua.Folder)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<User?> GetUserById(Guid id)
        {
            return dbContext.Users
                .Include(u => u.UserAccesses)
                    .ThenInclude(ua => ua.Folder)
                        .ThenInclude(f => f.InnerFolders)
                .FirstOrDefaultAsync(u => u.IdentityGuid == id);
        }
    }
}
