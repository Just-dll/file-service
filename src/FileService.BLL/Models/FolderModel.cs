using FileService.BLL.Models.Short;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.BLL.Models
{
    public class FolderModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<FolderShortModel> InnerFolders { get; internal set; } = [];
        public ICollection<FileShortModel> Files { get; internal set; } = [];

        public bool IsValid()
        {
            // Check if the folder name contains any invalid characters.
            return !Path.GetInvalidPathChars().Any(c => this.Name.Contains(c));
        }
    }
}
