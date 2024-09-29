using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Entities
{
    public class UserAccess 
    {
        public required uint UserId { get; set; }    
        public required uint FolderId { get; set; }
        public required AccessPermission AccessFlags { get; set; } = AccessPermission.Read;
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        [ForeignKey(nameof(FolderId))]
        public Folder? Folder { get; set; }
    }

    [Flags]
    public enum AccessPermission
    {
        Create = 1,
        Read = 2,
        Update = 4,
        Delete = 8,
        Owner = 16
    }
}
