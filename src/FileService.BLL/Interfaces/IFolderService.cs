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
    public interface IFolderService
    {
        Task<FolderModel?> GetFolderAsync(uint folderId);
        Task<FolderArchiveModel?> GetFolderArchiveAsync(uint folderId);
        Task<FolderModel> CreateFolderAsync(string name, uint? parentFolderId = null);
        Task UpdateFolderAsync(uint folderId, FolderModel folder);
        Task DeleteFolderAsync(uint folderId);
        Task<IEnumerable<FolderShortModel>> GetFullFolderPathAsync(uint folderId);
    }
}
