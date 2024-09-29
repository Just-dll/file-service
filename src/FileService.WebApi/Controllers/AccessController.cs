using FileService.BLL.Interfaces;
using FileService.BLL.Models;
using FileService.BLL.Models.Short;
using FileService.DAL.Entities;
using FileService.WebApi.Extensions;
using FileService.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FileService.WebApi.Controllers;

[Authorize]
[ApiController]
[ServiceFilter<FolderAccessFilter>]
[Route("api/Folder/{folderId}/[controller]")]
public class AccessController : ControllerBase
{
    private readonly IAccessService _accessService;
    public AccessController(IAccessService accessService)
    {
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccessModel>>> GetFolderAccessors(uint folderId)
    {
        return Ok(await _accessService.GetAccessors(folderId));
    }

    [HttpGet("/api/Access")]
    public async Task<ActionResult<IEnumerable<FolderModel>>> GetMyAccessedFolders()
    {
        Guid? userIdentifier = HttpContext.User.GetPrincipalIdentifier();
        if (userIdentifier == null)
        {
            return Unauthorized();
        }

        var userFolderId = HttpContext.GetUserFolderId();
        return Ok((await _accessService.GetAccessibleFoldersAsync(userIdentifier.Value)).Where(fm => fm.Id != userFolderId));
    }

    [HttpGet("/api/Folder/{folderId}/path")]
    public async Task<ActionResult<IEnumerable<FolderShortModel>>> GetAccessedFilePath(uint folderId)
    {
        Guid? userIdentifier = HttpContext.User.GetPrincipalIdentifier();
        if (userIdentifier == null)
        {
            return Unauthorized();
        }

        return Ok(await _accessService.GetFolderAccessiblePath(userIdentifier.Value, folderId));
    }

    [HttpPost]
    public async Task<ActionResult<AccessModel>> PostAccess(uint folderId, AccessShortModel model)
    {
        if (folderId == HttpContext.GetUserFolderId())
        {
            return Forbid();
        }

        if (model.Email == HttpContext.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value)
        {
            return BadRequest("You cannot give access to yourself");
        }

        if(model.Permission.HasFlag(AccessPermission.Owner))
        {
            return BadRequest("You cannot change or share the ownership");
        }

        try
        {
            return CreatedAtAction(nameof(PostAccess), await _accessService.GiveAccess(folderId, model));
        }
        catch(KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch(DbUpdateException)
        {
            return Conflict();
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAccess(uint folderId, AccessShortModel model)
    {
        if (folderId == HttpContext.GetUserFolderId())
        {
            return Forbid();
        }

        await _accessService.UpdateAccess(folderId, model);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccess(uint folderId, string email)
    {
        if(folderId == HttpContext.GetUserFolderId())
        {
            return Forbid();
        }

        await _accessService.DeleteAccess(folderId, email);
        return NoContent();
    }
}