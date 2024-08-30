using FileService.BLL.Grpc;
using FileService.BLL.Interfaces;
using FileService.DAL.Data;
using FileService.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileService.BLL.Grpc.Identity;

namespace FileService.BLL.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IdentityClient identityClient;
        private readonly FileServiceDbContext dbContext;
        public IdentityService(IdentityClient identityClient, FileServiceDbContext dbContext)
        {
            this.identityClient = identityClient;
            this.dbContext = dbContext;
        }
        public async Task<User?> GetUserByEmail(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if(user != null)
            {
                return user;
            }

            var request = new UserEmailRequest { Email = email };

            var response = await identityClient.GetUserByEmailAsync(request);

            if(response.Guid == default)
            {
                return null;
            }

            var tempUser = new User { Email = email, IdentityGuid = Guid.Parse(response.Guid), UserName = response.UserName };

            dbContext.Users.Add(tempUser);
            await dbContext.SaveChangesAsync();

            return tempUser;
        }

        public async Task<User?> GetUserById(Guid id)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.IdentityGuid == id);

            if (user != null)
            {
                return user;
            }

            var request = new UserRequest { UserId = id.ToString() };

            UserInstanceResponse response = await identityClient.GetUserAsync(request);

            if(response.Guid == default)
            {
                return null;
            }

            var tempUser = new User { Email = response.Email, IdentityGuid = Guid.Parse(response.Guid), UserName = response.UserName };
            dbContext.Users.Add(tempUser);
            await dbContext.SaveChangesAsync();

            return tempUser;
        }

    }
}
