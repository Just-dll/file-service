using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var downloadFile = await _fileService.DownloadFile(folderId, id);
        if (downloadFile == null)
        {
            return NotFound();
        }

        return File(downloadFile.Data, "application/octet-stream", downloadFile.Name);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(uint folderId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest();
        }

        try
        {
            var uploadedFile = await _fileService.UploadFileAsync(file, folderId);
            return CreatedAtAction(nameof(UploadFile), new { uploadedFile.Id });
        }
        catch(IOException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(uint folderId, uint id)
    {
        await _fileService.DeleteFileAsync(folderId, id);
        return NoContent();
    }
}
