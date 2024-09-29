using FileService.BLL.Interfaces;
using FileService.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace FileService.WebApi.Filters
{
    public class FolderAccessFilter : IAsyncActionFilter
    {
        private readonly IAccessService _accessService;
        public FolderAccessFilter(IAccessService accessService)
        {
            _accessService = accessService;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.User.GetPrincipalIdentifier();
            if (userId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            
            // Check if the action method has a 'folderId' parameter
            if (context.ActionArguments.TryGetValue("folderId", out var folderIdObj) && folderIdObj != null)
            {
                var folderId = Convert.ToUInt32(folderIdObj);

                var requiredPermission = context.HttpContext.Request.Method.ToUpper() switch
                {
                    "POST" => AccessPermission.Create,
                    "PUT" => AccessPermission.Update,
                    "DELETE" => AccessPermission.Delete,
                    _ => AccessPermission.Read,
                };

                // Check access to the folder
                if (!await _accessService.GetAccessVerification(userId.Value, folderId, requiredPermission))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}
