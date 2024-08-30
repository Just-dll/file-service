using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.DAL.Entities;
using FileService.WebApi.Extensions;
using FileService.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FileService.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ServiceFilter<FolderAccessFilter>]
public class FolderController : ControllerBase
{
    private readonly IFolderService _folderService;
    public FolderController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [HttpGet]
    public async Task<ActionResult<FolderModel>> GetMyFolder()
    {
        var folderId = HttpContext.GetUserFolderId();
        var result = await _folderService.GetFolderAsync(folderId);

        return Ok(result);
    }
    
    [HttpGet("{folderId}")]
    public async Task<ActionResult<FolderModel?>> GetFolder(uint folderId)
    {
        var folder = await _folderService.GetFolderAsync(folderId);

        if (folder == null)
        {
            return NotFound();
        }

        return Ok(folder);
    }

    [HttpPost]
    public async Task<IActionResult> PostFolder(uint? folderId, FolderModel model)
    {
        if (!model.IsValid())
        {
            return BadRequest();
        }

        folderId ??= HttpContext.GetUserFolderId();
        var result = await _folderService.CreateFolderAsync(model.Name, folderId);
        return CreatedAtAction(nameof(PostFolder), result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFolder(uint folderId, FolderModel model)
    {
        if(model.Id != folderId || !model.IsValid() || model.Id == HttpContext.GetUserFolderId())
        {
            return BadRequest();
        }

        try
        {
            await _folderService.UpdateFolderAsync(folderId, model);
            return NoContent();
        }
        catch(KeyNotFoundException)
        {
            return NotFound();
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{folderId}")]
    public async Task<IActionResult> DeleteFolder(uint folderId)
    {
        if (folderId == HttpContext.GetUserFolderId())
        {
            return Forbid();
        }

        await _folderService.DeleteFolderAsync(folderId);
        return NoContent();
    }
}