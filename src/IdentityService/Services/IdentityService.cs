using Grpc.Core;
using IdentityService.Models;
using MesaProject.IdentityService.Grpc;
using Microsoft.AspNetCore.Identity;

namespace MesaProject.IdentityService.Services
{
    public class IdentityService : Identity.IdentityBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public override async Task<IsUserExistsResponse> IsUserExists(UserRequest request, ServerCallContext context)
        {
            var user = await userManager.FindByIdAsync(request.UserId);

            return new() { Exists = user != null };
        }

        public override async Task<UserInstanceResponse> GetUser(UserRequest request, ServerCallContext context)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                context.Status = new Status(StatusCode.NotFound, "user not found");
                return new();
            }

            return new() { Email = user.Email, Guid = user.Id, UserName = user.UserName };
        }

        public override async Task<UserInstanceResponse> GetUserByEmail(UserEmailRequest request, ServerCallContext context)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                context.Status = new Status(StatusCode.NotFound, "user not found");
                return new();
            }

            return new() { Email = user.Email, Guid = user.Id, UserName = user.UserName };
        }
    }
}
