using FileService.DAL.Data;
using FileService.DAL.Entities;
using MesaProject.ResearchService.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MesaProject.ResearchService.Extensions;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class IdentityCheckerMiddleware
{
    private readonly RequestDelegate _next;

    public IdentityCheckerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, FileServiceDbContext context)
    {
        try
        {
            var principalIdentifier = httpContext.User.GetPrincipalIdentifier() ?? throw new UnauthorizedAccessException();

            var user = context.Users
                .Include(u => u.UserAccesses)
                    .ThenInclude(ua => ua.Folder)
                .FirstOrDefault(u => u.IdentityGuid == principalIdentifier);

            if (user == null)
            {
                user = new User
                {
                    UserName = httpContext.User.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                    Email = httpContext.User.Claims.First(c => c.Type == "email").Value,
                    IdentityGuid = principalIdentifier,
                };

                context.Users.Add(user);
                var folder = new Folder() { Name = principalIdentifier.ToString() };
                context.Folders.Add(folder);
                await context.SaveChangesAsync();
                context.UserAccesses.Add(new() 
                    { 
                        FolderId = folder.Id, 
                        UserId = user.Id, 
                        AccessFlags = AccessPermission.Create | AccessPermission.Read | AccessPermission.Update | AccessPermission.Delete 
                    });
                await context.SaveChangesAsync();
            }

            httpContext.Items.Add(nameof(User), user);
        }
        catch (Exception)
        {

        }
        finally
        {
            await _next(httpContext);
        }
    }
}

public static class IdentityMiddlewareExtensions
{
    public static IApplicationBuilder UseIdentityMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdentityCheckerMiddleware>();
    }
}
