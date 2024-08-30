using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.DAL.Entities
{
    public class File : BaseEntity
    {
        public required uint FolderId { get; set; }
        public required string InternalFilePath { get; set; }
        public required string Name { get; set; }
        public long Size { get; set; }
        public required DateTime CreationDate { get; set; }
        [ForeignKey(nameof(FolderId))]
        public Folder? Folder { get; set; }
    }
}
