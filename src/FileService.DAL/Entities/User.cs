using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Entities
{
    public class User : BaseEntity
    {
        public required Guid IdentityGuid { get; set; }
        public string? UserName { get; set; }
        public required string Email { get; set; }
        public ICollection<UserAccess> UserAccesses { get; } = [];
    }
}
