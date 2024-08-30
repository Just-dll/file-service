using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Entities
{
    public class Folder : BaseEntity
    {
        public uint? FolderId { get; set; }
        public required string Name { get; set; }
        [ForeignKey(nameof(FolderId))]
        public Folder? OuterFolder { get; set; }
        public ICollection<UserAccess> Accessors { get; } = [];
        public ICollection<File> Files { get; } = [];
        public ICollection<Folder> InnerFolders { get; } = [];
    }
}
