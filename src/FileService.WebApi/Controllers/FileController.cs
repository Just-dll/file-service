using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace FileService.WebApi.Controllers;

[Authorize]
[ApiController]
[ServiceFilter<FolderAccessFilter>]
[Route("api/Folder/{folderId}/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService) : base()
    {
        _fileService = fileService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FileModel?>> GetFile(uint folderId, uint id)
    {
        var file = await _fileService.GetFileAsync(folderId, id);
        if(file == null)
        {
            return NotFound();
        }

        return Ok(file);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(uint folderId, uint id)
    {
        try
        {
            var downloadFile = await _fileService.DownloadFile(folderId, id);
            if (downloadFile == null)
            {
                return NotFound();
            }

            return File(downloadFile.Data, "application/octet-stream", downloadFile.Name);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id}/preview")]
    public async Task<IActionResult> PreviewFile(uint folderId, uint id)
    {
        try
        {
            var filePreview = await _fileService.GetFilePreviewAsync(folderId, id);
            if (filePreview == null)
            {
                return NotFound();
            }

            return File(filePreview.Data, filePreview.ContentType, filePreview.Name);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
        {
            return NotFound();
        }
    }


    [HttpPost]
    public async Task<IActionResult> UploadFile(uint folderId, List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files provided.");
        }

        var uploadedFiles = new List<FileShortModel>();
        var failedFiles = new List<string>();

        foreach (var file in files)
        {
            try
            {
                var uploadedFile = await _fileService.UploadFileAsync(file, folderId);
                uploadedFiles.Add(uploadedFile);
            }
            catch (Exception ex)
            {
                failedFiles.Add(file.FileName);
                Console.WriteLine($"File upload failed for {file.FileName}: {ex.Message}");
            }
        }

        if (failedFiles.Count > 0)
        {
            return StatusCode(StatusCodes.Status207MultiStatus, new
            {
                message = "Some files failed to upload.",
                failedFiles
            });
        }

        return CreatedAtAction(nameof(UploadFile), new { uploadedFiles });
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(uint folderId, uint id)
    {
        await _fileService.DeleteFileAsync(folderId, id);
        return NoContent();
    }
}
