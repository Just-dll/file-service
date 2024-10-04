using FileService.DAL.Interfaces;
using FileService.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDALServices(this IServiceCollection services)
        {
            return services.AddScoped<IUnitOfWork, UnitOfWork>()
                .AddScoped<IFileRepository, FileRepository>()
                .AddScoped<IFolderRepository, FolderRepository>()
                .AddScoped<IUserAccessRepository, UserAccessRepository>()
                .AddScoped<IUserRepository, UserRepository>();
        }
    }
}
