using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Interfaces
{
    public interface IAccessService
    {
        Task<IEnumerable<FolderModel>> GetAccessibleFoldersAsync(Guid userId);
        Task<bool> GetAccessVerification(Guid userId, uint folderId, AccessPermission requiredPermission);
        Task<IEnumerable<AccessModel>> GetAccessors(uint folderId);
        Task<AccessModel> GiveAccess(uint folderId, AccessShortModel model);
        Task<AccessModel> UpdateAccess(uint folderId, AccessShortModel model);
        Task DeleteAccess(uint folderId, string email);
    }
}
