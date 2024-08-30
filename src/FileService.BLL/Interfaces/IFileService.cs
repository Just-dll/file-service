using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace FileService.BLL.Interfaces;

public interface IFileService
{
    Task<FileModel?> GetFileAsync(uint folderId, uint fileId);

    Task<FileDownloadModel?> DownloadFile(string filePath, string fileName);
    Task<FileDownloadModel?> DownloadFile(uint folderId, uint fileId);

    Task<FileShortModel> UploadFileAsync(IFormFile file, string filePath);
    Task<FileShortModel> UploadFileAsync(IFormFile file, uint folderId);

    Task DeleteFileAsync(string filePath, string fileName);
    Task DeleteFileAsync(uint folderId, uint fileId);
}