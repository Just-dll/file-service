using FileService.DAL.Entities;

namespace FileService.WebApi.Extensions
{
    public static class HttpContextExtensions
    {
        public static uint GetUserFolderId(this HttpContext context)
        {
            var userObject = context.Items[nameof(User)];
            if(userObject != null && userObject is User user)
            {
                return user.UserAccesses.First(ua => ua.Folder?.Name == user.IdentityGuid.ToString()).FolderId;
            }

            throw new UnauthorizedAccessException();
        }
    }
}
